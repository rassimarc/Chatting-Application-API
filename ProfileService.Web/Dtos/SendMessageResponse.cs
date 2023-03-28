using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record SendMessageResponse(
    UnixDateTime time
);