using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    partial class SetupWizardGarminCategoryAssociationControl : ExtendedWizardPageControl
    {
        public SetupWizardGarminCategoryAssociationControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            Options.Instance.OptionsChanged += new Options.OptionsChangedEventHandler(OnOptionsChanged);

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("CategoryAssociationExplanationText");
            ParentCategoryRadioButton.Text = GarminFitnessView.GetLocalizedString("UseParentCategoryText");
            CustomCategoryRadioButton.Text = GarminFitnessView.GetLocalizedString("UseCustomCategoryText");
            RunningRadioButton.Text = GarminFitnessView.GetLocalizedString("RunningText");
            CyclingRadioButton.Text = GarminFitnessView.GetLocalizedString("BikingText");
            OtherRadioButton.Text = GarminFitnessView.GetLocalizedString("OtherText");

            // Fill category list
            IApplication app = PluginMain.GetApplication();
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();

            for (int i = 0; i < app.Logbook.ActivityCategories.Count; ++i)
            {
                IActivityCategory currentCategory = app.Logbook.ActivityCategories[i];
                STToGarminActivityCategoryWrapper newNode = new STToGarminActivityCategoryWrapper(null, currentCategory);

                categories.Add(newNode);
                AddCategoryNode(newNode, null);
            }

            ActivityCategoryList.RowData = categories;
            ActivityCategoryList.Columns.Clear();
            ActivityCategoryList.Columns.Add(new TreeList.Column("Name", GarminFitnessView.GetLocalizedString("CategoryText"),
                                                                 140, StringAlignment.Near));
            ActivityCategoryList.Columns.Add(new TreeList.Column("GarminCategory", "", 75, StringAlignment.Near));
        }

        void OnOptionsChanged(System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            ActivityCategoryList.Invalidate();

            if (ActivityCategoryList.Selected.Count > 0)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

                CategorySelectionPanel.Enabled = true;

                if (Options.Instance.IsCustomGarminCategory(selectedCategory))
                {
                    CustomCategoryRadioButton.Checked = true;

                    switch (Options.Instance.GetGarminCategory(selectedCategory))
                    {
                        case GarminCategories.Running:
                            RunningRadioButton.Checked = true;
                            break;
                        case GarminCategories.Biking:
                            CyclingRadioButton.Checked = true;
                            break;
                        case GarminCategories.Other:
                            OtherRadioButton.Checked = true;
                            break;
                    }
                }
                else
                {
                    CustomCategoryRadioButton.Checked = false;
                }
            }
            else
            {
                CategorySelectionPanel.Enabled = false;
            }
        }

        private void ActivityCategoryList_SelectedChanged(object sender, EventArgs e)
        {
            if (ActivityCategoryList.Selected.Count == 1)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

                CategorySelectionPanel.Enabled = true;
                CustomCategoryRadioButton.Checked = Options.Instance.IsCustomGarminCategory(selectedCategory);
                ParentCategoryRadioButton.Checked = !CustomCategoryRadioButton.Checked;
                ParentCategoryRadioButton.Enabled = selectedCategory.Parent != null;
                GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

                if (Options.Instance.IsCustomGarminCategory(selectedCategory))
                {
                    switch (Options.Instance.GetGarminCategory(selectedCategory))
                    {
                        case GarminCategories.Running:
                            RunningRadioButton.Checked = true;
                            break;
                        case GarminCategories.Biking:
                            CyclingRadioButton.Checked = true;
                            break;
                        case GarminCategories.Other:
                            OtherRadioButton.Checked = true;
                            break;
                    }
                }
            }
        }

        private void ParentCategoryRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            CustomCategoryRadioButton.Checked = !ParentCategoryRadioButton.Checked;
            GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

            if (ParentCategoryRadioButton.Checked)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

                Options.Instance.RemoveGarminCategory(selectedCategory);
                ActivityCategoryList.Invalidate();
            }
        }

        private void CustomCategoryRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ParentCategoryRadioButton.Checked = !CustomCategoryRadioButton.Checked;
            GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

            if (CustomCategoryRadioButton.Checked)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;
                GarminCategories parentCategory = Options.Instance.GetGarminCategory(selectedCategory);

                Options.Instance.SetGarminCategory(selectedCategory, parentCategory);

                switch (parentCategory)
                {
                    case GarminCategories.Running:
                        RunningRadioButton.Checked = true;
                        break;
                    case GarminCategories.Biking:
                        CyclingRadioButton.Checked = true;
                        break;
                    case GarminCategories.Other:
                        OtherRadioButton.Checked = true;
                        break;
                }
            }
        }

        private void RunningRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (RunningRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Running);
            }
        }

        private void CyclingRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (CyclingRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Biking);
            }
        }

        private void OtherRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (OtherRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Other);
            }
        }

        private void AddCategoryNode(STToGarminActivityCategoryWrapper categoryNode, STToGarminActivityCategoryWrapper parent)
        {
            IActivityCategory category = (IActivityCategory)categoryNode.Element;

            if (parent != null)
            {
                parent.Children.Add(categoryNode);
            }

            for (int i = 0; i < category.SubCategories.Count; ++i)
            {
                IActivityCategory currentCategory = category.SubCategories[i];
                STToGarminActivityCategoryWrapper newNode = new STToGarminActivityCategoryWrapper(categoryNode, currentCategory);

                AddCategoryNode(newNode, categoryNode);
            }
        }
    }
}
