using System;
using System.Collections;
using MettaFramework.Classes;
using MettaFramework.Forms;
using MettaFramework.MetaDataCashe;
using MettaFramework.UPanels;

namespace DiagramApp.Helpers
{
    public static class TaskNavigationHelper
    {
        public static void OpenTask(int? rowType, int? itemID, int? goods_TaskID, int? clientOrderID)
        {
            try
            {
                Guid objectID = new Guid();
                Hashtable inparams = new Hashtable();
                if (rowType.HasValue)
                {
                    if (rowType == 1) // Метаобъект - Заказы клиентов
                    {
                        objectID = new Guid("1e1931c0-d891-4c82-8304-d639dab30578");
                        inparams["ClientOrderID"] = clientOrderID;
                    }
                    else if (rowType == 2) // Метаобъект - Товары
                    {
                        objectID = new Guid("17d773a5-0154-4d92-ab2c-6b7c6c9869a5");
                        inparams["ItemID"] = itemID;
                        inparams["Goods_TaskID"] = goods_TaskID;
                    }
                    OpenCard(objectID, inparams);
                }
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при открытии карточки Объекта!", error);
            }
        }

        public static void OpenCard(Guid ObjectID, Hashtable inparams)
        {
            try
            {
                // Карточка класса
                MetaObject mo = new MetaObject(ObjectID);
                ObjectCardPanel pnl = new ObjectCardPanel();
                pnl.Init(mo, inparams, null);
                pnl.Dock = System.Windows.Forms.DockStyle.Fill;

                TabForm frm = new TabForm();
                frm.Init(pnl, mo);
                frm.MdiParent = Common.MainForm;
                frm.Show();
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при открытии карточки Объекта!", error);
            }
        }

    }
}
