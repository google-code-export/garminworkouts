using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminWorkoutPlugin.View
{
    public partial class SelectCategoryDialog : Form
    {
        public SelectCategoryDialog(string workoutName, CultureInfo UICulture)
        {
            InitializeComponent();

            this.Text = m_ResourceManager.GetString("SelectCategoryDialogText", UICulture) + workoutName;
            SelectCategoryLabel.Text = this.Text;
            OkButton.Text = m_ResourceManager.GetString("OKButtonText", UICulture);

            // Fill list
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();
            List<TreeList.TreeListNode> selection = new List<TreeList.TreeListNode>();

            for (int i = 0; i < PluginMain.GetApplication().Logbook.ActivityCategories.Count; ++i)
            {
                IActivityCategory currentCategory = PluginMain.GetApplication().Logbook.ActivityCategories[i];
                ActivityCategoryWrapper newNode = new ActivityCategoryWrapper(null, currentCategory);

                categories.Add(newNode);
                AddCategoryNode(newNode, null);

                if (i == 0)
                {
                    selection.Add(newNode);
                }
            }

            ActivityCategoryList.RowData = categories;
            ActivityCategoryList.Columns.Clear();
            ActivityCategoryList.Columns.Add(new TreeList.Column("Name", m_ResourceManager.GetString("CategoryText", UICulture),
                                                         150, StringAlignment.Near));
            ActivityCategoryList.Selected = selection;
            ActivityCategoryList.SetExpanded(ActivityCategoryList.RowData, true, true);
        }

        private void ActivityCategoryList_DoubleClick(object sender, System.EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void AddCategoryNode(ActivityCategoryWrapper categoryNode, ActivityCategoryWrapper parent)
        {
            IActivityCategory category = (IActivityCategory)categoryNode.Element;

            if (parent != null)
            {
                parent.Children.Add(categoryNode);
            }

            for (int i = 0; i < category.SubCategories.Count; ++i)
            {
                IActivityCategory currentCategory = category.SubCategories[i];
                ActivityCategoryWrapper newNode = new ActivityCategoryWrapper(categoryNode, currentCategory);

                AddCategoryNode(newNode, categoryNode);
            }
        }

        public IActivityCategory SelectedCategory
        {
            get { return (IActivityCategory)((ActivityCategoryWrapper)ActivityCategoryList.SelectedItems[0]).Element; }
        }

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
    }

}