using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVentas.Entity;


namespace SistemaVenta.BLL.Implementacion
{
    public class NegocioService : INegocioService
    {

        private readonly IGenericRepository<Negocio> _repositorio;
        private readonly IFireBaseService _firebaseService;
        
        public NegocioService(IGenericRepository<Negocio> repositorio, IFireBaseService firebaseService)
        {
            _repositorio = repositorio;
            _firebaseService = firebaseService;
        }

        public async Task<Negocio> Obtener()
        {
            try
            {
                Negocio negocioEncontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);
                return negocioEncontrado; 
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Negocio> GuardarCambios(Negocio entidad, Stream Logo = null, string Nombrelogo = "")
        {
            try
            {
                Negocio negocioEncontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);

                negocioEncontrado.NumeroDocumento = entidad.NumeroDocumento;
                negocioEncontrado.Nombre = entidad.Nombre;
                negocioEncontrado.Correo = entidad.Correo;
                negocioEncontrado.Direccion = entidad.Direccion;
                negocioEncontrado.Telefono = entidad.Telefono;
                negocioEncontrado.PorcentajeImpuesto = entidad.PorcentajeImpuesto;
                negocioEncontrado.SimboloMoneda = entidad.SimboloMoneda;

                negocioEncontrado.NombreLogo = negocioEncontrado.NombreLogo == "" ? Nombrelogo : negocioEncontrado.NombreLogo;

                if (Logo != null)
                {
                    string urlLogo = await _firebaseService.SubirStorage(Logo, "carpeta_logo", negocioEncontrado.NombreLogo);

                    negocioEncontrado.UrlLogo = urlLogo;
                }

                await _repositorio.Editar(negocioEncontrado);

                return negocioEncontrado;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
