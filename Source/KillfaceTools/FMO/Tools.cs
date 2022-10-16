using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace KillfaceTools.FMO;

public static class Tools
{
    public const string NestedString = " ►";

    public static FloatMenuLabels LabelMenu;

    private static FloatMenuNested actionMenu;

    public static void CloseLabelMenu(bool sound)
    {
        if (LabelMenu == null)
        {
            return;
        }

        Find.WindowStack.TryRemove(LabelMenu, sound);
        LabelMenu = null;
    }

    public static FloatMenuOption MakeMenuItemForLabel([NotNull] string label, [NotNull] List<FloatMenuOption> fmo)
    {
        // List<SortByWhat> sortByWhats = fmo.Keys.ToList();
        var options = fmo.ToList();
        var isSingle = options.Count == 1 && !label.Contains(NestedString);

        var option = new FloatMenuOptionNoClose(
            label,
            () =>
            {
                if (isSingle && options[0].Disabled == false)
                {
                    var action = options[0].action;
                    if (action == null)
                    {
                        return;
                    }

                    CloseLabelMenu(true);
                    action();
                }
                else
                {
                    var i = 0;
                    var actions = new List<FloatMenuOption>();
                    fmo.ForEach(
                        menuOption =>
                        {
                            var floatMenuOption = new FloatMenuOption(
                                menuOption.Label,
                                () =>
                                {
                                    actionMenu.Close();
                                    CloseLabelMenu(true);
                                    menuOption.action();
                                },
                                (MenuOptionPriority)i++,
                                menuOption.mouseoverGuiAction,
                                menuOption.revalidateClickTarget,
                                menuOption.extraPartWidth,
                                menuOption.extraPartOnGUI);
                            actions.Add(floatMenuOption);
                        });
                    actionMenu = new FloatMenuNested(actions, null);
                    Find.WindowStack.Add(actionMenu);
                }
            },
            isSingle ? options[0].extraPartWidth : 0f,
            isSingle ? options[0].extraPartOnGUI : null)
        {
            Disabled = options.All(o => o.Disabled)
        };
        return option;
    }
}