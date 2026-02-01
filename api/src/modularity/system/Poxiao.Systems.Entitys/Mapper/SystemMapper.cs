using Mapster;
using Poxiao.Infrastructure.Dtos.DataBase;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.Dto.Database;
using Poxiao.Systems.Entitys.Dto.DbBackup;
using Poxiao.Systems.Entitys.Dto.ModuleColumn;
using Poxiao.Systems.Entitys.Dto.ModuleDataAuthorize;
using Poxiao.Systems.Entitys.Dto.ModuleForm;
using Poxiao.Systems.Entitys.Dto.System.InterfaceOauth;
using Poxiao.Systems.Entitys.Model.DataBase;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Entitys.System;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using SqlSugar;
using Yitter.IdGenerator;
using static DingTalk.Api.Response.OapiV2DepartmentListsubResponse;
using static DingTalk.Api.Response.OapiV2UserListResponse;

namespace Poxiao.Systems.Entitys.Mapper;

/// <summary>
/// 系统模块对象映射.
/// </summary>
public class SystemMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<DbBackupEntity, DbBackupListOutput>()
               .Map(dest => dest.fileUrl, src => src.FilePath);
        config.ForType<DbColumnInfo, DbTableFieldModel>()
            .Map(dest => dest.field, src => src.DbColumnName)
            .Map(dest => dest.fieldName, src => src.ColumnDescription)
            .Map(dest => dest.dataLength, src => src.Length.ToString())
            .Map(dest => dest.identity, src => src.IsIdentity ? true : false)
            .Map(dest => dest.primaryKey, src => src.IsPrimarykey ? 1 : 0)
            .Map(dest => dest.allowNull, src => src.IsNullable ? 1 : 0)
            .Map(dest => dest.decimalDigits, src => src.DecimalDigits)
            .Map(dest => dest.defaults, src => src.DefaultValue);
        config.ForType<DbTableFieldModel, DbColumnInfo>()
            .Map(dest => dest.DbColumnName, src => src.field)
            .Map(dest => dest.ColumnDescription, src => src.fieldName)
            .Map(dest => dest.Length, src => int.Parse(src.dataLength))
            .Map(dest => dest.IsIdentity, src => src.identity)
            .Map(dest => dest.IsPrimarykey, src => src.primaryKey)
            .Map(dest => dest.IsNullable, src => src.allowNull == 1)
            .Map(dest => dest.DecimalDigits, src => src.decimalDigits)
            .Map(dest => dest.DefaultValue, src => src.defaults);
        config.ForType<DynamicDbTableModel, DbTableModel>()
            .Map(dest => dest.table, src => src.FTABLE)
            .Map(dest => dest.tableName, src => src.FTABLENAME)
            .Map(dest => dest.size, src => src.FSIZE)
            .Map(dest => dest.sum, src => int.Parse(src.FSUM))
            .Map(dest => dest.primaryKey, src => src.FPRIMARYKEY);
        config.ForType<DynamicDbTableModel, DatabaseTableListOutput>()
          .Map(dest => dest.table, src => src.FTABLE.IsNullOrEmpty() ? string.Empty : src.FTABLE)
          .Map(dest => dest.tableName, src => src.FTABLENAME.IsNullOrEmpty() ? string.Empty : src.FTABLENAME)
          .Map(dest => dest.sum, src => int.Parse(src.FSUM));
        config.ForType<DbTableInfo, DatabaseTableListOutput>()
            .Map(dest => dest.table, src => src.Name)
            .Map(dest => dest.tableName, src => src.Description);
        config.ForType<DbTableInfo, DbTableModel>()
            .Map(dest => dest.table, src => src.Name)
            .Map(dest => dest.tableName, src => src.Description);
        config.ForType<DbTableInfo, TableInfoOutput>()
            .Map(dest => dest.table, src => src.Name)
            .Map(dest => dest.tableName, src => src.Description);
        config.ForType<DbColumnInfo, TableFieldOutput>()
            .Map(dest => dest.field, src => src.DbColumnName)
            .Map(dest => dest.fieldName, src => src.ColumnDescription)
            .Map(dest => dest.dataLength, src => src.Length.ToString())
            .Map(dest => dest.primaryKey, src => src.IsPrimarykey ? 1 : 0)
            .Map(dest => dest.allowNull, src => src.IsNullable ? 1 : 0);
        config.ForType<ModuleColumnEntity, ModuleColumnListOutput>()
                .Map(dest => dest.enCode, src => src.EnCode.Replace("poxiao_" + src.BindTable + "_poxiao_", string.Empty));
        config.ForType<ModuleDataAuthorizeEntity, ModuleDataAuthorizeListOutput>()
            .Map(dest => dest.enCode, src => src.EnCode.Replace("poxiao_" + src.BindTable + "_poxiao_", string.Empty));
        config.ForType<ModuleFormEntity, ModuleFormListOutput>()
            .Map(dest => dest.enCode, src => src.EnCode.Replace("poxiao_" + src.BindTable + "_poxiao_", string.Empty));
        config.ForType<GetMemberResult, UserEntity>()
             .Map(dest => dest.Id, src => YitIdHelper.NextId().ToString())
             .Map(dest => dest.Account, src => src.userid)
             .Map(dest => dest.RealName, src => src.name)
             .Map(dest => dest.QuickQuery, src => PinyinHelper.PinyinString(src.name))
             .Map(dest => dest.HeadIcon, src => "001.png")
             .Map(dest => dest.Secretkey, src => "26916bdf390242c9b0ac7ec1442a329e")
             .Map(dest => dest.Password, src => "045cbd671a8d67d2110a0b6098025551")
             .Map(dest => dest.MobilePhone, src => src.mobile)
             .Map(dest => dest.NickName, src => src.alias)
             .Map(dest => dest.OrganizeId, src => src.main_department.ToString())
             .Map(dest => dest.EnabledMark, src => src.enable)
             .Map(dest => dest.PropertyJson, src => src.extattr.ToJsonString())
             .Map(dest => dest.PostalAddress, src => src.address);
        config.ForType<ListUserResponseDomain, UserEntity>()
             .Map(dest => dest.Id, src => YitIdHelper.NextId().ToString())
             .Map(dest => dest.Account, src => src.Userid)
             .Map(dest => dest.RealName, src => src.Name)
             .Map(dest => dest.QuickQuery, src => PinyinHelper.PinyinString(src.Name))
             .Map(dest => dest.HeadIcon, src => "001.png")
             .Map(dest => dest.Secretkey, src => "26916bdf390242c9b0ac7ec1442a329e")
             .Map(dest => dest.Password, src => "045cbd671a8d67d2110a0b6098025551")
             .Map(dest => dest.MobilePhone, src => src.Mobile)
             .Map(dest => dest.OrganizeId, src => src.DeptIdList.Last().ToString())
             .Map(dest => dest.EnabledMark, src => 1)
             .Map(dest => dest.PropertyJson, src => src.Extension.ToJsonString())
             .Map(dest => dest.PostalAddress, src => src.WorkPlace);
        config.ForType<DepartmentList, OrganizeEntity>()
           .Map(dest => dest.Id, src => src.id.ToString())
           .Map(dest => dest.ParentId, src => src.parentid == 0 ? "-1" : src.parentid.ToString())
           .Map(dest => dest.Category, src => src.parentid == 0 ? "company" : "department")
           .Map(dest => dest.EnCode, src => PinyinHelper.PinyinString(src.name))
           .Map(dest => dest.FullName, src => src.name)
            .Map(dest => dest.SortCode, src => 0);
        config.ForType<DeptBaseResponseDomain, OrganizeEntity>()
            .Map(dest => dest.Id, src => src.DeptId)
            .Map(dest => dest.ParentId, src => src.ParentId.ToString())
            .Map(dest => dest.EnCode, src => PinyinHelper.PinyinString(src.Name))
            .Map(dest => dest.FullName, src => src.Name)
             .Map(dest => dest.SortCode, src => 0);
        config.ForType<InterfaceOauthInput, InterfaceOauthEntity>()
            .Map(dest => dest.UsefulLife, src => src.usefulLife.TimeStampToDateTime());
    }
}