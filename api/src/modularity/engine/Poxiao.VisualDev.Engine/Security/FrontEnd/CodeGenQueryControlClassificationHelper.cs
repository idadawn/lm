using Poxiao.Infrastructure.Const;

namespace Poxiao.VisualDev.Engine.Security;

/// <summary>
/// 代码生成查询控件归类帮助类.
/// </summary>
public class CodeGenQueryControlClassificationHelper
{
    /// <summary>
    /// 列表查询控件.
    /// </summary>
    /// <param name="type">1-Web设计,2-App设计,3-流程表单,4-Web表单,5-App表单.</param>
    /// <returns></returns>
    public static Dictionary<string, List<string>> ListQueryControl(int type)
    {
        Dictionary<string, List<string>> listQueryControl = new Dictionary<string, List<string>>();
        switch (type)
        {
            case 4:
                {
                    var useInputList = new List<string>();
                    useInputList.Add(PoxiaoKeyConst.COMINPUT);
                    useInputList.Add(PoxiaoKeyConst.TEXTAREA);
                    useInputList.Add(PoxiaoKeyConst.PoxiaoTEXT);
                    useInputList.Add(PoxiaoKeyConst.BILLRULE);
                    listQueryControl["inputList"] = useInputList;

                    var useDateList = new List<string>();
                    useDateList.Add(PoxiaoKeyConst.CREATETIME);
                    useDateList.Add(PoxiaoKeyConst.MODIFYTIME);
                    listQueryControl["dateList"] = useDateList;

                    var useSelectList = new List<string>();
                    useSelectList.Add(PoxiaoKeyConst.SELECT);
                    useSelectList.Add(PoxiaoKeyConst.RADIO);
                    useSelectList.Add("checkbox");
                    listQueryControl["selectList"] = useSelectList;

                    var timePickerList = new List<string>();
                    timePickerList.Add(PoxiaoKeyConst.TIME);
                    listQueryControl["timePickerList"] = timePickerList;

                    var numRangeList = new List<string>();
                    numRangeList.Add(PoxiaoKeyConst.NUMINPUT);
                    numRangeList.Add(PoxiaoKeyConst.CALCULATE);
                    listQueryControl["numRangeList"] = numRangeList;

                    var datePickerList = new List<string>();
                    datePickerList.Add(PoxiaoKeyConst.DATE);
                    listQueryControl["datePickerList"] = datePickerList;

                    var userSelectList = new List<string>();
                    userSelectList.Add(PoxiaoKeyConst.CREATEUSER);
                    userSelectList.Add(PoxiaoKeyConst.MODIFYUSER);
                    userSelectList.Add(PoxiaoKeyConst.USERSELECT);
                    listQueryControl["userSelectList"] = userSelectList;

                    var usersSelectList = new List<string>();
                    usersSelectList.Add(PoxiaoKeyConst.USERSSELECT);
                    listQueryControl["usersSelectList"] = usersSelectList;

                    var comSelectList = new List<string>();
                    comSelectList.Add(PoxiaoKeyConst.COMSELECT);
                    comSelectList.Add(PoxiaoKeyConst.CURRORGANIZE);
                    listQueryControl["comSelectList"] = comSelectList;

                    var depSelectList = new List<string>();
                    depSelectList.Add(PoxiaoKeyConst.CURRDEPT);
                    depSelectList.Add(PoxiaoKeyConst.DEPSELECT);
                    listQueryControl["depSelectList"] = depSelectList;

                    var posSelectList = new List<string>();
                    posSelectList.Add(PoxiaoKeyConst.CURRPOSITION);
                    posSelectList.Add(PoxiaoKeyConst.POSSELECT);
                    listQueryControl["posSelectList"] = posSelectList;

                    var useCascaderList = new List<string>();
                    useCascaderList.Add(PoxiaoKeyConst.CASCADER);
                    listQueryControl["useCascaderList"] = useCascaderList;

                    var poxiaoAddressList = new List<string>();
                    poxiaoAddressList.Add(PoxiaoKeyConst.ADDRESS);
                    listQueryControl["PoxiaoAddressList"] = poxiaoAddressList;

                    var treeSelectList = new List<string>();
                    treeSelectList.Add(PoxiaoKeyConst.TREESELECT);
                    listQueryControl["treeSelectList"] = treeSelectList;

                    var autoCompleteList = new List<string>();
                    autoCompleteList.Add(PoxiaoKeyConst.AUTOCOMPLETE);
                    listQueryControl["autoCompleteList"] = autoCompleteList;
                }

                break;
            case 5:
                {
                    var inputList = new List<string>();
                    inputList.Add(PoxiaoKeyConst.COMINPUT);
                    inputList.Add(PoxiaoKeyConst.TEXTAREA);
                    inputList.Add(PoxiaoKeyConst.PoxiaoTEXT);
                    inputList.Add(PoxiaoKeyConst.BILLRULE);
                    inputList.Add(PoxiaoKeyConst.CALCULATE);
                    listQueryControl["input"] = inputList;

                    var numRangeList = new List<string>();
                    numRangeList.Add(PoxiaoKeyConst.NUMINPUT);
                    listQueryControl["numRange"] = numRangeList;

                    var switchList = new List<string>();
                    switchList.Add(PoxiaoKeyConst.SWITCH);
                    listQueryControl["switch"] = switchList;

                    var selectList = new List<string>();
                    selectList.Add(PoxiaoKeyConst.RADIO);
                    selectList.Add(PoxiaoKeyConst.CHECKBOX);
                    selectList.Add(PoxiaoKeyConst.SELECT);
                    listQueryControl["select"] = selectList;

                    var cascaderList = new List<string>();
                    cascaderList.Add(PoxiaoKeyConst.CASCADER);
                    listQueryControl["cascader"] = cascaderList;

                    var timeList = new List<string>();
                    timeList.Add(PoxiaoKeyConst.TIME);
                    listQueryControl["time"] = timeList;

                    var dateList = new List<string>();
                    dateList.Add(PoxiaoKeyConst.DATE);
                    dateList.Add(PoxiaoKeyConst.CREATETIME);
                    dateList.Add(PoxiaoKeyConst.MODIFYTIME);
                    listQueryControl["date"] = dateList;

                    var comSelectList = new List<string>();
                    comSelectList.Add(PoxiaoKeyConst.COMSELECT);
                    listQueryControl["comSelect"] = comSelectList;

                    var depSelectList = new List<string>();
                    depSelectList.Add(PoxiaoKeyConst.DEPSELECT);
                    depSelectList.Add(PoxiaoKeyConst.CURRDEPT);
                    depSelectList.Add(PoxiaoKeyConst.CURRORGANIZE);
                    listQueryControl["depSelect"] = depSelectList;

                    var posSelectList = new List<string>();
                    posSelectList.Add(PoxiaoKeyConst.POSSELECT);
                    posSelectList.Add(PoxiaoKeyConst.CURRPOSITION);
                    listQueryControl["posSelect"] = posSelectList;

                    var userSelectList = new List<string>();
                    userSelectList.Add(PoxiaoKeyConst.USERSELECT);
                    userSelectList.Add(PoxiaoKeyConst.CREATEUSER);
                    userSelectList.Add(PoxiaoKeyConst.MODIFYUSER);
                    listQueryControl["userSelect"] = userSelectList;

                    var usersSelectList = new List<string>();
                    usersSelectList.Add(PoxiaoKeyConst.USERSSELECT);
                    listQueryControl["usersSelect"] = usersSelectList;

                    var treeSelectList = new List<string>();
                    treeSelectList.Add(PoxiaoKeyConst.TREESELECT);
                    listQueryControl["treeSelect"] = treeSelectList;

                    var addressList = new List<string>();
                    addressList.Add(PoxiaoKeyConst.ADDRESS);
                    listQueryControl["address"] = addressList;

                    listQueryControl["autoComplete"] = new List<string> { PoxiaoKeyConst.AUTOCOMPLETE };

                    listQueryControl["groupSelect"] = new List<string>() { PoxiaoKeyConst.GROUPSELECT };

                    listQueryControl["roleSelect"] = new List<string>() { PoxiaoKeyConst.ROLESELECT };
                }

                break;
        }

        return listQueryControl;
    }
}