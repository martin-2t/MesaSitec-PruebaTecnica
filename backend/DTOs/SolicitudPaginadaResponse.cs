namespace backend.DTOs;

public class SolicitudPaginadaResponse
{
    public List<SolicitudListResponse> Items { get; set; } = new();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int Total { get; set; }

    public int TotalPaginas { get; set; }
}