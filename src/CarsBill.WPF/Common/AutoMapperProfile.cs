using AutoMapper;
using CarsBill.WPF.Models;

namespace CarsBill.WPF.Common;

/// <summary>
/// AutoMapper 映射配置
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // 车辆信息 - 实体与自身映射（后续添加DTO时替换）
        CreateMap<CarInfo, CarInfo>();

        // 地点信息
        CreateMap<AddressInfo, AddressInfo>();

        // 物料信息
        CreateMap<MaterialInfo, MaterialInfo>();

        // 项目信息
        CreateMap<ProjectInfo, ProjectInfo>();

        // 业务记录
        CreateMap<BusinessRecord, BusinessRecord>();

        // 单价规则
        CreateMap<PriceRule, PriceRule>();
    }
}
