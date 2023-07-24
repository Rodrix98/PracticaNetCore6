using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVentas.Entity;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IProductoService _productoServicio;

        public ProductoController(IMapper mapper, IProductoService productoServicio)
        {
            _mapper = mapper;
            _productoServicio = productoServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMProducto> vmProductoLista = _mapper.Map<List<VMProducto>>(await _productoServicio.Lista());

            return StatusCode(StatusCodes.Status200OK, new { data = vmProductoLista });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();

            try
            {
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                string nombreImagen = "";
                Stream imagenStream = null;

                if (imagen != null)
                {
                    string nombreCodigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(imagen.FileName);
                    nombreImagen = string.Concat(nombreCodigo, extension);
                    imagenStream = imagen.OpenReadStream();
                }

                Producto productoCreado = await _productoServicio.Crear(_mapper.Map<Producto>(vmProducto), imagenStream, nombreImagen);

                vmProducto = _mapper.Map<VMProducto>(productoCreado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;
            }
            catch (Exception ex)
            {

                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();

            try
            {
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                string nombreImagen = "";

                Stream imagenStream = null;

                if (imagen != null)
                {
                    string nombreCodigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(imagen.FileName);
                    nombreImagen = string.Concat(nombreCodigo, extension);
                    imagenStream = imagen.OpenReadStream();
                }

                Producto productoEditado = await _productoServicio.Editar(_mapper.Map<Producto>(vmProducto), imagenStream, nombreImagen);

                vmProducto = _mapper.Map<VMProducto>(productoEditado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;
            }
            catch (Exception ex)
            {

                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idProducto)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();

            try
            {
                gResponse.Estado = await _productoServicio.Eliminar(idProducto);
            }
            catch (Exception ex)
            {

                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
