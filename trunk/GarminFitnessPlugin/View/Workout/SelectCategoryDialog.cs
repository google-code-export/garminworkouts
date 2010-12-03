using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    public partial class SelectCategoryDialog : Form
    {
        public SelectCategoryDialog(string workoutName)
        {
            InitializeComponent();

            this.Text = GarminFitnessView.GetLocalizedString("SelectCategoryDialogText") + workoutName;
            SelectCategoryLabel.Text = this.Text;
            UseCategoryForAllCheckBox.Text = GarminFitnessView.GetLocalizedString("UseCategoryForAllCheckBoxText");
            OkButton.Text = CommonResources.Text.ActionOk;

            // Fill list
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();
            List<TreeList.TreeListNode> selection = new List<TreeList.TreeListNode>();

            foreach (IActivityCategory currentCategory in PluginMain.GetApplication().Logbook.ActivityCategories)
            {
                if (Options.Instance.GetVisibleInWorkoutList(currentCategory))
                {
                    ActivityCategoryWrapper childSelection = null;
                    ActivityCategoryWrapper newNode = new ActivityCategoryWrapper(null, currentCategory);

                    categories.Add(newNode);
                    childSelection = AddCategoryNode(newNode, null);

                    if (Options.Instance.LastImportCategory == currentCategory)
                    {
                        selection.Add(newNode);
                    }
                    else if (childSelection != null)
                    {
                        selection.Add(childSelection);
                    }
                }
            }

            if (selection.Count == 0)
            {
                selection.Add(categories[0]);
            }

            ActivityCategoryList.RowData = categories;
            ActivityCategoryList.Columns.Clear();
            ActivityCategoryList.Columns.Add(new TreeList.Column("Name", GarminFitnessView.GetLocalizedString("CategoryText"),
                                                         150, StringAlignment.Near));
            ActivityCategoryList.Selected = selection;
        }

        private void ActivityCategoryList_DoubleClick(object sender, System.EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private ActivityCategoryWrapper AddCategoryNode(ActivityCategoryWrapper categoryNode, ActivityCategoryWrapper parent)
        {
            IActivityCategory category = (IActivityCategory)categoryNode.Element;
            ActivityCategoryWrapper selection = null;

            if (parent != null)
            {
                parent.Children.Add(categoryNode);

                if (Options.Instance.LastImportCategory == category)
                {
                    selection = categoryNode;
                }
            }

            foreach (IActivityCategory currentCategory in category.SubCategories)
            {
                if (Options.Instance.GetVisibleInWorkoutList(currentCategory))
                {
                    ActivityCategoryWrapper childSelection = null;
                    ActivityCategoryWrapper newNode = new ActivityCategoryWrapper(categoryNode, currentCategory);

                    childSelection = AddCategoryNode(newNode, categoryNode);

                    if (childSelection != null)
                    {
                        selection = childSelection;
                    }
                }
            }

            return selection;
        }

        public IActivityCategory SelectedCategory
        {
            get { return (IActivityCategory)((ActivityCategoryWrapper)ActivityCategoryList.SelectedItems[0]).Element; }
        }

        private void UseCategoryForAllCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseLastCategoryForAllImportedWorkout = UseCategoryForAllCheckBox.Checked;
        }
    }

}