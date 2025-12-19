using System;

namespace identity_service.Dtos.Menu;

public class UpdateMenusBySystemDto
{    
    public required List<UpdateMenuBatchDto> Menus { get; set; }
}
