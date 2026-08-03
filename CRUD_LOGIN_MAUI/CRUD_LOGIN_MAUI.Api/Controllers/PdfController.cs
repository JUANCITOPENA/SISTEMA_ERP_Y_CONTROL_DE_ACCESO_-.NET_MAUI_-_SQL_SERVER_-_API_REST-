using Microsoft.AspNetCore.Mvc;
using CRUD_LOGIN_MAUI.Api.Models;
using CRUD_LOGIN_MAUI.Api.Services;

namespace CRUD_LOGIN_MAUI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        [HttpPost("ticket")]
        public IActionResult GenerarTicket([FromBody] TicketRequest request)
        {
            var generator = new TicketPdfGenerator();
            var pdfBytes = generator.GenerarPdf(request);
            return File(pdfBytes, "application/pdf", $"Ticket_{request.NumeroVenta}.pdf");
        }
    }
}
