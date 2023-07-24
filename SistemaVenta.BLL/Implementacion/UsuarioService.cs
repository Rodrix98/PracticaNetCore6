using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVentas.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {

        private readonly IGenericRepository<Usuario> _repositorio;

        private readonly IFireBaseService _fireBaseService;

        private readonly IUtilidadesService _utilidadesService;

        private readonly ICorreoService _correoService;

        public UsuarioService(IGenericRepository<Usuario> repositorio, IFireBaseService fireBaseService, IUtilidadesService utilidadesService, ICorreoService correoService)
        {
            _repositorio = repositorio;
            _fireBaseService = fireBaseService;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
        }

        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList();
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream foto = null, string nombreFoto = "", string urlPlantillaCorreo = "")
        {
            Usuario usuarioExiste = await _repositorio.Obtener(u => u.Correo == entidad.Correo);

            if (usuarioExiste != null)
            {

                throw new TaskCanceledException("El correo ya existe");

            }

            try
            {

                string claveGenerada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSHA256(claveGenerada);
                entidad.NombreFoto = nombreFoto;

                if (foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(foto, "carpeta_usuario", nombreFoto);
                    entidad.UrlFoto = urlFoto;
                }

                Usuario usuarioCreado = await _repositorio.Crear(entidad);

                if (usuarioCreado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear el usuario");
                }

                if (urlPlantillaCorreo !="")
                {
                    urlPlantillaCorreo = urlPlantillaCorreo.Replace("[correo]", usuarioCreado.Correo).Replace("[clave]", claveGenerada);

                    string htmlCorreo = "";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlPlantillaCorreo);

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using(Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader readerStream = null;

                            if (response.CharacterSet == null)
                            {
                                readerStream = new StreamReader(dataStream);
                            }
                            else
                            {
                                readerStream = new StreamReader(dataStream,Encoding.GetEncoding(response.CharacterSet));

                                htmlCorreo = readerStream.ReadToEnd();

                                response.Close();

                                readerStream.Close();
                            }
                        } 

                    }

                    if (htmlCorreo != "")
                    {
                        await _correoService.EnviarCorreo(usuarioCreado.Correo, "Cuenta Creada", htmlCorreo);
                    }

                    IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);
                    usuarioCreado = query.Include(r => r.IdRolNavigation).First();

                   
                }

                return usuarioCreado;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<Usuario> Editar(Usuario entidad, Stream foto = null, string nombreFoto = "")
        {
            Usuario usuarioExiste = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);

            if (usuarioExiste != null)
            {
                throw new TaskCanceledException("El correo ya existe");
            }

            try
            {
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);

                Usuario usuarioEditar = queryUsuario.First();

                usuarioEditar.Nombre = entidad.Nombre;
                usuarioEditar.Correo = entidad.Correo;
                usuarioEditar.Telefono = entidad.Telefono;
                usuarioEditar.IdRol = entidad.IdRol;
                usuarioEditar.EsActivo = entidad.EsActivo;

                if (usuarioEditar.NombreFoto == "")
                {
                    usuarioEditar.NombreFoto = nombreFoto;
                }

                if (foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(foto, "carpeta_usuario", usuarioEditar.NombreFoto);
                    usuarioEditar.UrlFoto = urlFoto;
                }

                bool respuesta = await _repositorio.Editar(usuarioEditar);

                if (!respuesta)
                {
                    throw new TaskCanceledException("No se pudo modificar el usuario");
                }

                Usuario usuarioEditado = queryUsuario.Include(r => r.IdRolNavigation).First();

                return usuarioEditado;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> Eliminar(int idUsuario)
        {

            try
            {

                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.IdUsuario == idUsuario);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                string nombreFoto = usuarioEncontrado.NombreFoto;

                bool respuesta = await _repositorio.Eliminar(usuarioEncontrado);

                if (respuesta)
                {
                    await _fireBaseService.EliminarStorage("carpeta_usuario", nombreFoto);
                }

                return true;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {

            string claveEncriptada = _utilidadesService.ConvertirSHA256(clave);

            Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.Correo.Equals(correo) && u.Clave.Equals(claveEncriptada));

            return usuarioEncontrado;

        }

        public async Task<Usuario> ObtenerPorID(int idUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == idUsuario);

            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();

            return resultado;
        }

        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {

                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                usuarioEncontrado.Correo = entidad.Correo;
                usuarioEncontrado.Telefono = entidad.Telefono;

                bool respuesta = await _repositorio.Editar(usuarioEncontrado);

                return respuesta;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> CambiarClave(int idUsuario, string ClaveActual, string ClaveNueva)
        {

            try
            {

                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.IdUsuario == idUsuario);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                if (usuarioEncontrado.Clave != _utilidadesService.ConvertirSHA256(ClaveActual))
                {
                    throw new TaskCanceledException("La contraseña ingresada como actual no es correcta");
                }

                usuarioEncontrado.Clave = _utilidadesService.ConvertirSHA256(ClaveNueva);

                bool respuesta = await _repositorio.Editar(usuarioEncontrado);

                return respuesta;


            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {

            try
            {

                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.Correo == Correo);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("No encontramos ningun usuario asociado al correo");
                }

                string claveGenerada = _utilidadesService.GenerarClave();

                usuarioEncontrado.Clave = _utilidadesService.ConvertirSHA256(claveGenerada);

                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", claveGenerada);

                string htmlCorreo = "";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;

                        if (response.CharacterSet == null)
                        {
                            readerStream = new StreamReader(dataStream);
                        }
                        else
                        {
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                            htmlCorreo = readerStream.ReadToEnd();

                            response.Close();

                            readerStream.Close();
                        }
                    }

                }

                bool correoEnviado = false;

                if (htmlCorreo != "")
                {
                    await _correoService.EnviarCorreo(Correo, "Contraseña Restablecida", htmlCorreo);
                }

                if (!correoEnviado)
                {
                    throw new TaskCanceledException("Tenemos problemas, porfavor intentalo de nuevo mas tarde");
                }


                bool respuesta = await _repositorio.Editar(usuarioEncontrado);

                return respuesta;

            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
