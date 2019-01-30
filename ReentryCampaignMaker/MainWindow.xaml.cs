using Microsoft.Win32;
using Newtonsoft.Json;
using Reentry.Campaign;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReentryCampaignMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CampaignData campaignData;

        const string CAMPAIGN_DESCRIPTOR_NAME = "*.desc";

        public MainWindow()
        {
            InitializeComponent();
            campaignData = new CampaignData();
        }

        void AddHeaderSection(string content = "Header")
        {
            TextBlock header = new TextBlock();
            header.Name = "headerSection" + sectionItemId;
            header.Text = content;
            header.TextWrapping = TextWrapping.Wrap;
            header.FontSize = 30;
            header.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            SectionHolder.Items.Add(header);
        }

        void AddTextSection(string content = "This is some text.")
        {

            TextBlock text = new TextBlock();
            text.Name = "textSection" + sectionItemId;
            text.TextWrapping = TextWrapping.Wrap;
            text.Text = content;
            text.FontSize = 20;
            text.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            SectionHolder.Items.Add(text);
        }

        void AddImageSection(string imageFileName = "image.png")
        {
            Image img = new Image();
            img.Name = "imageSection" + sectionItemId;
            if(!string.IsNullOrEmpty(imageFileName))
            {
                try
                {
                    img.Source = new BitmapImage(new Uri(CampaignWorkingFolder.Text + "\\" + imageFileName, UriKind.Absolute));
                } catch
                {
                    img.Source = new BitmapImage(new Uri(@"image.png", UriKind.Relative));
                }
            }
            else img.Source= new BitmapImage(new Uri(@"image.png"));

            SectionHolder.Items.Add(img);
        }

        int sectionItemId = 0;
        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            switch (SectionTypeSelector.SelectedIndex)
            {
                case 0:
                    {
                        AddHeaderSection();
                        break;
                    }
                case 1:
                    {
                        AddTextSection();
                        break;
                    }
                case 2:
                    {
                        AddImageSection();
                        break;
                    }
            }
            sectionItemId++;
        }


        TextBlock workingWithContentTextBlock;
        Image workingWithContentImage;
        private void SectionHolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock selectedTextItem = SectionHolder.SelectedItem as TextBlock;

            if (selectedTextItem != null) // it's text based
            {
                workingWithContentTextBlock = selectedTextItem;
                if (selectedTextItem.Name.Contains("headerSection"))
                {
                    HandleHeaderSelection();
                }
                else if (selectedTextItem.Name.Contains("textSection"))
                {
                    HandleTextSelection();
                }

            } else // it's an image
            {
                Image selectedImageItem = SectionHolder.SelectedItem as Image;
                if (selectedImageItem != null)
                {
                    workingWithContentImage = selectedImageItem;
                    if (selectedImageItem.Name.Contains("imageSection"))
                    {
                        HandleImageSelection();
                    }
                }
            }
        }
        void ResetInspector()
        {
            InspectorHeader.Visibility = Visibility.Collapsed;
            InspectorText.Visibility = Visibility.Collapsed;
            InspectorImage.Visibility = Visibility.Collapsed;
            InspectorEmpty.Visibility = Visibility.Visible;
        }
        void HandleHeaderSelection()
        {
            ResetInspector();

            InspectorHeader.Visibility = Visibility.Visible;

            HeaderContent.Text = workingWithContentTextBlock.Text;
        }
        void HandleTextSelection()
        {
            ResetInspector();

            InspectorText.Visibility = Visibility.Visible;

            TextContent.Text = workingWithContentTextBlock.Text;
        }
        void HandleImageSelection()
        {
            ResetInspector();

            InspectorImage.Visibility = Visibility.Visible;
            ImageContent.Text = StripAbsoluteFileInRelativeCampaignFolder(workingWithContentImage.Source.ToString());
        }

        string StripAbsoluteFileInRelativeCampaignFolder(string path)
        {
            string imgSrc = path;
            imgSrc = imgSrc.Replace("file:///", "");
            string campaignFolder = CampaignWorkingFolder.Text;
            campaignFolder = campaignFolder.Replace("\\", "/");
            imgSrc = imgSrc.Replace(campaignFolder, "");
            if (imgSrc[0] == '/')
                imgSrc = imgSrc.Remove(0, 1);

            return imgSrc;
        }

        private void HeaderContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(workingWithContentTextBlock != null)
            {
                workingWithContentTextBlock.Text = HeaderContent.Text;
            }
        }

        private void TextContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (workingWithContentTextBlock != null)
            {
                workingWithContentTextBlock.Text = TextContent.Text;
            }
        }

        private void SetWorkingFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                CampaignWorkingFolder.Text = folderDialog.SelectedPath;
            }

            if(CheckIfDescriptorExistsInWorkingFolder())
            {
                if(MoreThanOneDescriptor())
                {
                    MessageBox.Show("More than one descriptors was found. Please remove one of them and try again.");
                }
                BlockerGrid.Visibility = Visibility.Collapsed;
                LoadDescriptor();
            } else
            {
                CreateNewCampaign();

                BlockerGrid.Visibility = Visibility.Collapsed;
                MessageBox.Show("New campaign created!");
            }
        }

        void GenerateControllersFromDescriptorFile()
        {
            if(campaignData != null)
            {
                sectionItemId = 0;
                SectionHolder.Items.Clear();
                foreach (var section in campaignData.Sections)
                {
                    if(section.Type == CampaignSection.CampaignSectionType.Header)
                    {
                        AddHeaderSection(section.Data);
                    }
                    if (section.Type == CampaignSection.CampaignSectionType.Text)
                    {
                        AddTextSection(section.Data);
                    }
                    if (section.Type == CampaignSection.CampaignSectionType.Image)
                    {
                        AddImageSection(section.Data);
                    }
                }
            }
        }
        void LoadDescriptor()
        {
            if (LoadDescriptorJson())
            {
                GenerateControllersFromDescriptorFile();
                CampaignTitle.Text = campaignData.Title;
                if(campaignData.PageBackgroundImage != null)
                {
                    if(campaignData.PageBackgroundImage != "")
                    {
                        SetBackground(CampaignWorkingFolder.Text + "\\" + campaignData.PageBackgroundImage);
                    }
                }
                if (campaignData.TileBackgroundImage != null)
                {
                    if (campaignData.TileBackgroundImage != "")
                    {
                        TileBGImagePreview.Source = new BitmapImage(new Uri(CampaignWorkingFolder.Text + "\\" + campaignData.TileBackgroundImage));
                        TileBGImage.Text = StripAbsoluteFileInRelativeCampaignFolder(TileBGImagePreview.Source.ToString());
                    }
                }

                SetMissionsGrid();
                // loading complete
            }
            else
            {
                // loading failed
            }
        }

        FileInfo descriptorFile;
        void SetDescriptorFile()
        {
            if(CheckIfDescriptorExistsInWorkingFolder())
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(CampaignWorkingFolder.Text);
                FileInfo[] fileInfo = directoryInfo.GetFiles("*.desc", SearchOption.TopDirectoryOnly);
                descriptorFile = fileInfo[0];
            }
        }
        bool MoreThanOneDescriptor()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(CampaignWorkingFolder.Text);
            FileInfo[] fileInfo = directoryInfo.GetFiles("*.desc", SearchOption.TopDirectoryOnly);
            if (fileInfo.Length > 1)
                return true;
            return false;
        }
        bool CheckIfDescriptorExistsInWorkingFolder()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(CampaignWorkingFolder.Text);
            FileInfo[] fileInfo = directoryInfo.GetFiles("*.desc", SearchOption.TopDirectoryOnly);
            if (fileInfo.Length > 0)
                return true;
            return false;
        }

        string descriptorJson = "";
        bool LoadDescriptorJson()
        {
            if(CheckIfDescriptorExistsInWorkingFolder())
            {
                SetDescriptorFile();

                string customMisisonJson = File.ReadAllText(descriptorFile.FullName);
                try
                {
                    campaignData = JsonConvert.DeserializeObject<CampaignData>(customMisisonJson);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException.ToString());
                }
                if(campaignData != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetImageFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                workingWithContentImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                ImageContent.Text = StripAbsoluteFileInRelativeCampaignFolder(workingWithContentImage.Source.ToString());
            }
        }

        private void RemoveSection_Click(object sender, RoutedEventArgs e)
        {
            if(SectionHolder.SelectedIndex != -1)
            {
                ResetInspector();
                SectionHolder.Items.RemoveAt(SectionHolder.SelectedIndex);
            }
        }
        
        private void ButtonMoveUp_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.SectionHolder.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = this.SectionHolder.Items[selectedIndex];
                this.SectionHolder.Items.RemoveAt(selectedIndex);
                this.SectionHolder.Items.Insert(selectedIndex - 1, itemToMoveUp);
                this.SectionHolder.SelectedIndex = selectedIndex - 1;
            }
        }

        private void ButtonMoveDown_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.SectionHolder.SelectedIndex;

            if (selectedIndex + 1 < this.SectionHolder.Items.Count)
            {
                var itemToMoveDown = this.SectionHolder.Items[selectedIndex];
                this.SectionHolder.Items.RemoveAt(selectedIndex);
                this.SectionHolder.Items.Insert(selectedIndex + 1, itemToMoveDown);
                this.SectionHolder.SelectedIndex = selectedIndex + 1;
            }
        }

        private void TileBGImageSetter_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TileBGImagePreview.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                TileBGImage.Text = StripAbsoluteFileInRelativeCampaignFolder(TileBGImagePreview.Source.ToString());
            }
        }

        private void PageBGImageSetter_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SetBackground(openFileDialog.FileName);
            }
        }

        void SetBackground(string bg)
        {
            ImageBrush myBrush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(bg))
            };
            this.Background = myBrush;
            PageBGImage.Text = StripAbsoluteFileInRelativeCampaignFolder(myBrush.ImageSource.ToString());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateFullCampaignDataFile();

            string json = JsonConvert.SerializeObject(campaignData);

            File.WriteAllText(descriptorFile.FullName, json);
            MessageBox.Show("Campaign saved!");
        }

        void CreateNewCampaign()
        {
            campaignData = new CampaignData();
            campaignData.Missions.Add(new CampaignMissionData() { Program = CampaignMissionData.CampaignMissionProgram.Mercury, Title = "Mission 1", FileName = "mission1.json" });

            string json = JsonConvert.SerializeObject(campaignData);
            File.WriteAllText(CampaignWorkingFolder.Text + "\\campaign.desc", json);

            SetDescriptorFile();
        }

        void GenerateFullCampaignDataFile()
        {
            List<CampaignMissionData> missions = campaignData.Missions;

            campaignData = new CampaignData()
            {
                Title = CampaignTitle.Text,
                TileBackgroundImage = TileBGImage.Text,
                PageBackgroundImage = PageBGImage.Text,
            };

            campaignData.Sections = new List<CampaignSection>();
            foreach(var section in SectionHolder.Items)
            {
                CampaignSection newSection = new CampaignSection();

                TextBlock selectedTextItem = section as TextBlock;
                if (selectedTextItem != null) // it's text based
                {
                    if (selectedTextItem.Name.Contains("headerSection"))
                    {
                        newSection.Type = CampaignSection.CampaignSectionType.Header;
                        newSection.Data = selectedTextItem.Text;
                    }
                    else if (selectedTextItem.Name.Contains("textSection"))
                    {
                        newSection.Type = CampaignSection.CampaignSectionType.Text;
                        newSection.Data = selectedTextItem.Text;
                    }
                }
                else // it's an image
                {
                    Image selectedImageItem = section as Image;
                    if (selectedImageItem != null)
                    {
                        if (selectedImageItem.Name.Contains("imageSection"))
                        {
                            newSection.Type = CampaignSection.CampaignSectionType.Image;
                            newSection.Data = StripAbsoluteFileInRelativeCampaignFolder(selectedImageItem.Source.ToString());
                        }
                    }
                }

                campaignData.Sections.Add(newSection);
            }

            campaignData.Missions = missions;
        }


        private void GridLoaded(object sender, RoutedEventArgs e)
        {
            SetMissionsGrid();
        }
        void SetMissionsGrid()
        {
            MissionsGrid.ItemsSource = null;
            MissionsGrid.ItemsSource = campaignData.Missions;
        }

        private void CloseMissions_Click(object sender, RoutedEventArgs e)
        {
            MissionPopup.Visibility = Visibility.Collapsed;
        }

        private void SetMissionsButton_Click(object sender, RoutedEventArgs e)
        {
            SetMissionsGrid();
            MissionPopup.Visibility = Visibility.Visible;
        }
    }
}
