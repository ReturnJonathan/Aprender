using RaymiMusic.Modelos;
using static System.Net.WebRequestMethods;

namespace RaymiMusic.AppWeb.Services
{
    public class PlanesService : IPlanesService
    {
        private readonly HttpClient _http;
        public  PlanesService(HttpClient http) => _http = http;


        public async Task RealizarPago(Pago pago)
        {
            var response = await _http.PostAsJsonAsync("api/Pagos", pago);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"[{response.StatusCode}] {body}");
            }
        }


        public async Task<PlanSuscripcion> ObtenerPlanUsuarioAsync(Guid UserId)
        {
            var plan = await _http.GetFromJsonAsync<PlanSuscripcion>($"api/Planes/User/{UserId}");
            return plan ?? throw new Exception("Artista no encontrado");


        }
        public async Task AsignarPlan(Guid userId, string nombrePlan)
        {
            var response = await _http.PostAsJsonAsync($"api/Planes/Asignar/{userId}/{nombrePlan}", new { });
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al asignar el plan");
            }
        }
        public async Task<PlanSuscripcion> ObtenerPlanPorNombreAsync(string nombrePlan)
        {
            var plan = await _http.GetFromJsonAsync<PlanSuscripcion>($"api/Planes/Nombre/{nombrePlan}");
            return plan ?? throw new Exception("Plan no encontrado");
        }
    }
}
