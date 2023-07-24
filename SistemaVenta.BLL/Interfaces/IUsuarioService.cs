﻿using SistemaVentas.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IUsuarioService
    {

        Task<List<Usuario>> Lista();

        Task<Usuario> Crear(Usuario entidad, Stream foto = null, string nombreFoto = "", string urlPlantillaCorreo = "");

        Task<Usuario> Editar(Usuario entidad, Stream foto = null, string nombreFoto = "");

        Task<bool> Eliminar(int idUsuario);

        Task<Usuario> ObtenerPorCredenciales(string correo, string clave);

        Task<Usuario> ObtenerPorID(int idUsuario);

        Task<bool> GuardarPerfil(Usuario entidad);

        Task<bool> CambiarClave(int idUsuario, string ClaveActual, string ClaveNueva);

        Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo);



    }
}
