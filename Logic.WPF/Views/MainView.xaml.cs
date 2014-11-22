﻿using Logic.Core;
using Logic.Graph;
using Logic.Simulation;
using Logic.WPF.Page;
using Logic.WPF.Serialization;
using Logic.WPF.Simulation;
using Logic.WPF.Templates;
using Logic.WPF.Util;
using Logic.WPF.Util.Parts;
using Logic.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace Logic.WPF.Views
{
    public partial class MainView : Window
    {
        #region Properties

        public MainViewModel Model { get; set; }

        #endregion

        #region Fields

        private IStringSerializer _serializer = null;
        private XLayers _layers = null;
        private IRenderer _renderer = null;
        private ITemplate _template = null;
        private Point _dragStartPoint;
        private bool _isContextMenu = false;
        private System.Threading.Timer _timer = null;
        private Clock _clock = null;

        #endregion

        #region Visual Parent

        public T FindVisualParent<T>(DependencyObject child) 
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            return FindVisualParent<T>(parentObject);
        } 

        #endregion

        #region Constructor

        public MainView()
        {
            InitializeComponent();

            InitializeModel();
            InitializePage();
            InitializeBlocks();
            InitializeMEF();
        }

        #endregion

        #region Initialize

        private void InitializeModel()
        {
            Model = new MainViewModel();

            Model.Blocks = new ObservableCollection<XBlock>();
            Model.Templates = new ObservableCollection<ITemplate>();

            Model.FileName = null;
            Model.FilePath = null;

            Model.Tool = new ToolMenuModel();

            Model.FileNewCommand = new Command(
                (parameter) => this.New(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileOpenCommand = new Command
                ((parameter) => this.Open(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileSaveCommand = new Command(
                (parameter) => this.Save(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileSaveAsCommand = new Command(
                (parameter) => this.SaveAs(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileExitCommand = new Command(
                (parameter) => 
                {
                    if (IsSimulationRunning())
                    {
                        this.Stop();
                    }
                    this.Close();
                }, 
                (parameter) => true);

            Model.EditUndoCommand = new Command(
                (parameter) => this.Undo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.History.CanUndo() ? false : true;
                });

            Model.EditRedoCommand = new Command
                ((parameter) => this.Redo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.History.CanRedo() ? false : true;
                });

            Model.EditCutCommand = new Command(
                (parameter) => this.Cut(),
                (parameter) => 
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.CanCopy() ? false : true;
                });

            Model.EditCopyCommand = new Command(
                (parameter) => this.Copy(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.CanCopy() ? false : true;
                });

            Model.EditPasteCommand = new Command(
                (parameter) => this.Paste(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.CanPaste() ? false : true;
                });

            Model.EditDeleteCommand = new Command(
                (parameter) => this.Delete(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.HaveSelected() ? false : true;
                });

            Model.EditSelectAllCommand = new Command(
                (parameter) => this.SelectAll(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftBottomCommand = new Command(
                (parameter) => this.AlignLeftBottom(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignBottomCommand = new Command(
                (parameter) => this.AlignBottom(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightBottomCommand = new Command(
                (parameter) => this.AlignRightBottom(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftCommand = new Command(
                (parameter) => this.AlignLeft(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignCenterCenterCommand = new Command(
                (parameter) => this.AlignCenterCenter(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightCommand = new Command(
                (parameter) => this.AlignRight(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftTopCommand = new Command(
                (parameter) => this.AlignLeftTop(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignTopCommand = new Command(
                (parameter) => this.AlignTop(), 
                (parameter) => IsSimulationRunning() ? false : true);
           
            Model.EditAlignRightTopCommand = new Command
                ((parameter) => this.AlignRightTop(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditIncreaseTextSizeCommand = new Command(
                (parameter) => this.IncreaseTextSize(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditDecreaseTextSizeCommand = new Command(
                (parameter) => this.DecreaseTextSize(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleFillCommand = new Command(
                (parameter) => this.ToggleFill(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleSnapCommand = new Command(
                (parameter) => this.ToggleSnap(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertStartCommand = new Command(
                (parameter) => this.ToggleInvertStart(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertEndCommand = new Command(
                (parameter) => this.ToggleInvertEnd(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditCancelCommand = new Command(
                (parameter) => this.Cancel(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolNoneCommand = new Command(
                (parameter) => this.SetToolNone(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolSelectionCommand = new Command(
                (parameter) => this.SetToolSelection(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolWireCommand = new Command(
                (parameter) => this.SetToolWire(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolPinCommand = new Command(
                (parameter) => this.SetToolPin(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolLineCommand = new Command(
                (parameter) => this.SetToolLine(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolEllipseCommand = new Command(
                (parameter) => this.SetToolEllipse(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolRectangleCommand = new Command(
                (parameter) => this.SetToolRectangle(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolTextCommand = new Command(
                (parameter) => this.SetToolText(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockImportCommand = new Command(
                (parameter) => this.ImportBlock(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockImportCodeCommand = new Command(
                (parameter) => this.ImportBlocksFromCode(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockExportCommand = new Command(
                (parameter) => this.ExportBlock(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.HaveSelected() ? false : true;
                });

            Model.BlockCreateProjectCommand = new Command(
                (parameter) => this.CreateProject(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_layers.Editor.HaveSelected() ? false : true;
                });

            Model.InsertBlockCommand = new Command(
                (parameter) =>
                {
                    XBlock block = parameter as XBlock;
                    if (block != null)
                    {
                        double x = _isContextMenu ? _layers.Editor.RightX : 0.0;
                        double y = _isContextMenu ? _layers.Editor.RightY : 0.0;
                        InsertBlock(block, x, y);
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateImportCommand = new Command(
                (parameter) => this.ImportTemplate(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateImportCodeCommand = new Command(
                (parameter) => this.ImportTemplatesFromCode(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateExportCommand = new Command(
                (parameter) => this.ExportTemplate(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ApplyTemplateCommand = new Command(
                (parameter) =>
                {
                    ITemplate template = parameter as ITemplate;
                    if (template != null)
                    {
                        ApplyTemplate(template, _renderer);
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationStartCommand = new Command(
                (parameter) => this.Start(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationStopCommand = new Command(
                (parameter) => this.Stop(), 
                (parameter) => IsSimulationRunning() ? true : false);

            Model.SimulationRestartCommand = new Command(
                (parameter) => this.Restart(), 
                (parameter) => IsSimulationRunning() ? true : false);

            Model.SimulationCreateGraphCommand = new Command(
                (parameter) => this.Graph(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationOptionsCommand = new Command(
                (parameter) => this.Options(), 
                (parameter) => IsSimulationRunning() ? false : true);
        }

        private void InitializePage()
        {
            // serializer
            _serializer = new Json();

            // layers
            _layers = new XLayers();
            _layers.Shapes = pageView.shapeLayer.Layer;
            _layers.Blocks = pageView.blockLayer.Layer;
            _layers.Wires = pageView.wireLayer.Layer;
            _layers.Pins = pageView.pinLayer.Layer;
            _layers.Editor = pageView.editorLayer.Layer;
            _layers.Overlay = pageView.overlayLayer.Layer;

            // editor
            _layers.Editor.Layers = _layers;

            // overlay
            _layers.Overlay.IsOverlay = true;

            // renderer
            _renderer = new NativeRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };

            _layers.Shapes.Renderer = _renderer;
            _layers.Blocks.Renderer = _renderer;
            _layers.Wires.Renderer = _renderer;
            _layers.Pins.Renderer = _renderer;
            _layers.Editor.Renderer = _renderer;
            _layers.Overlay.Renderer = _renderer;

            // template
            ApplyTemplate(new LogicPageTemplate(), _renderer);

            // history
            _layers.Editor.History = new History<XPage>();

            // tool
            _layers.Editor.Tool = Model.Tool;
            _layers.Editor.Tool.CurrentTool = ToolMenuModel.Tool.Selection;

            // drag & drop
            pageView.editorLayer.AllowDrop = true;

            pageView.editorLayer.DragEnter += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                if (!e.Data.GetDataPresent("Block") || s == e.Source)
                {
                    e.Effects = DragDropEffects.None;
                }
            };

            pageView.editorLayer.Drop += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                // block
                if (e.Data.GetDataPresent("Block"))
                {
                    try
                    {
                        XBlock block = e.Data.GetData("Block") as XBlock;
                        if (block != null)
                        {
                            Point point = e.GetPosition(pageView.editorLayer);
                            InsertBlock(block, point.X, point.Y);
                            e.Handled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError("{0}{1}{2}", 
                            ex.Message, 
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
                // files
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    try
                    {
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files != null && files.Length == 1)
                        {
                            string path = files[0];
                            if (!string.IsNullOrEmpty(path))
                            {
                                _layers.Editor.Load(path);
                                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                                Model.FilePath = path;
                                e.Handled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            };

            // context menu
            pageView.ContextMenuOpening += (s, e) =>
            {
                if (_layers.Editor.CurrentMode != XLayer.Mode.None)
                {
                    e.Handled = true;
                }
                else if (_layers.Editor.SkipContextMenu == true)
                {
                    _layers.Editor.SkipContextMenu = false;
                    e.Handled = true;
                }
                else
                {
                    _isContextMenu = true;
                }
            };

            pageView.ContextMenuClosing += (s, e) =>
            {
                _isContextMenu = false;
            };
        }

        private void InitializeBlocks()
        {
            blocks.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                _dragStartPoint = e.GetPosition(null);
            };

            blocks.PreviewMouseMove += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                Point point = e.GetPosition(null);
                Vector diff = _dragStartPoint - point;
                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    var listBox = s as ListBox;
                    var listBoxItem = FindVisualParent<ListBoxItem>(
                        (DependencyObject)e.OriginalSource);
                    if (listBoxItem != null)
                    {
                        var block = (XBlock)listBox
                            .ItemContainerGenerator
                            .ItemFromContainer(listBoxItem);
                        DataObject dragData = new DataObject("Block", block);
                        DragDrop.DoDragDrop(
                            listBoxItem, 
                            dragData, 
                            DragDropEffects.Move);
                    }
                }
            };
        }

        private void InitializeMEF()
        {
            try
            {
                var builder = new RegistrationBuilder();
                builder.ForTypesDerivedFrom<XBlock>().Export<XBlock>();
                builder.ForTypesDerivedFrom<ITemplate>().Export<ITemplate>();

                var catalog = new AggregateCatalog();

                catalog.Catalogs.Add(
                    new AssemblyCatalog(
                        Assembly.GetExecutingAssembly(), builder));

                if (System.IO.Directory.Exists("./Blocks"))
                {
                    catalog.Catalogs.Add(
                        new DirectoryCatalog("./Blocks", builder));
                }

                if (System.IO.Directory.Exists("./Templates"))
                {
                    catalog.Catalogs.Add(
                        new DirectoryCatalog("./Templates", builder));
                }

                var container = new CompositionContainer(catalog);
                container.ComposeParts(Model);
            }
            catch (CompositionException ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }

            this.DataContext = Model;
        }

        #endregion

        #region File

        private void New()
        {
            _layers.Editor.New();
            Model.FileName = null;
            Model.FilePath = null;
        }

        private void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog(this) == true)
            {
                _layers.Editor.Load(dlg.FileName);
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                Model.FilePath = dlg.FileName;
            }
        }

        private void Save()
        {
            if (!string.IsNullOrEmpty(Model.FilePath))
            {
                _layers.Editor.Save(Model.FilePath);
            }
            else
            {
                SaveAs();
            }
        }

        private void SaveAs()
        {
            string fileName = string.IsNullOrEmpty(Model.FilePath) ?
                "shapes" : System.IO.Path.GetFileName(Model.FilePath);

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Json (*.json)|*.json",
                FileName = fileName
            };

            if (dlg.ShowDialog(this) == true)
            {
                _layers.Editor.Save(dlg.FileName);
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                Model.FilePath = dlg.FileName;
            }
        }

        #endregion

        #region Edit

        private void Undo()
        {
            _layers.Editor.Undo();
        }

        private void Redo()
        {
            _layers.Editor.Redo();
        }

        private void Cut()
        {
            _layers.Editor.Cut();
        }

        private void Copy()
        {
            _layers.Editor.Copy();
        }

        private void Paste()
        {
            _layers.Editor.Paste();
            if (_isContextMenu && _renderer.Selected != null)
            {
                double minX = pageView.editorLayer.Width;
                double minY = pageView.editorLayer.Height;
                _layers.Editor.GetMin(_renderer.Selected, ref minX, ref minY);
                double x = _layers.Editor.RightX - minX;
                double y = _layers.Editor.RightY - minY;
                _layers.Editor.Move(_renderer.Selected, x, y);
            }
        }

        private void Delete()
        {
            _layers.Editor.SelectionDelete();
        }

        private void SelectAll()
        {
            _layers.Editor.SelectAll();
        }

        private void ToggleFill()
        {
            _layers.Editor.ToggleFill();
        }

        private void ToggleSnap()
        {
            _layers.Editor.EnableSnap = !_layers.Editor.EnableSnap;
        }

        private void ToggleInvertStart()
        {
            _layers.Editor.ToggleInvertStart();
        }

        private void ToggleInvertEnd()
        {
            _layers.Editor.ToggleInvertEnd();
        }

        private void IncreaseTextSize()
        {
            _layers.Editor.SetTextSizeDelta(+1.0);
        }

        private void DecreaseTextSize()
        {
            _layers.Editor.SetTextSizeDelta(-1.0);
        }

        private void AlignLeftBottom()
        {
            _layers.Editor.SetTextHAlignment(HAlignment.Left);
            _layers.Editor.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignBottom()
        {
            _layers.Editor.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignRightBottom()
        {
            _layers.Editor.SetTextHAlignment(HAlignment.Right);
            _layers.Editor.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignLeft()
        {
            _layers.Editor.SetTextHAlignment(HAlignment.Left);
        }

        private void AlignCenterCenter()
        {
            _layers.Editor.SetTextHAlignment(HAlignment.Center);
            _layers.Editor.SetTextVAlignment(VAlignment.Center);
        }

        private void AlignRight()
        {
            _layers.Editor.SetTextHAlignment(HAlignment.Right);
        }

        private void AlignLeftTop()
        {
            _layers.Editor.SetTextHAlignment(HAlignment.Left);
            _layers.Editor.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignTop()
        {
            _layers.Editor.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignRightTop()
        {
            _layers.Editor.SetTextHAlignment(HAlignment.Right);
            _layers.Editor.SetTextVAlignment(VAlignment.Top);
        }

        private void Cancel()
        {
            _layers.Editor.Cancel();
        }

        #endregion

        #region Tool

        private void SetToolNone()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.None;
        }

        private void SetToolSelection()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Selection;
        }

        private void SetToolLine()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Line;
        }

        private void SetToolEllipse()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Ellipse;
        }

        private void SetToolRectangle()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Rectangle;
        }

        private void SetToolText()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Text;
        }

        private void SetToolWire()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Wire;
        }

        private void SetToolPin()
        {
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Pin;
        }

        #endregion

        #region Block

        private void InsertBlock(XBlock block, double x, double y)
        {
            _layers.Editor.History.Snapshot(
                _layers.Editor.Layers.ToPage("Page", null));
            XBlock copy = _layers.Editor.Insert(block, x, y);
            if (copy != null)
            {
                _layers.Editor.Connect(copy);
            }
        }

        private void ImportBlock()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var block = OpenBlock(dlg.FileName);
                if (block != null)
                {
                    Model.Blocks.Add(block);
                }
            }
        }

        private void CreateProject()
        {
            var block = _layers.Editor.CreateBlockFromSelected("Block");
            if (block == null)
                return;

            var view = new CodeView();
            view.Owner = this;
            view.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var vm = new CodeViewModel()
            {
                NamespaceName = "Blocks.Name",
                ClassName = "Name",
                BlockName = "NAME",
                ProjectPath = "Blocks.Name.csproj"
            };

            vm.BrowseCommand = new Command((parameter) => 
            { 
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "C# Project (*.csproj)|*.csproj",
                    FileName = vm.ProjectPath
                };

                if (dlg.ShowDialog(view) == true)
                {
                    vm.ProjectPath = dlg.FileName;
                }
            },
            (parameter) => true);

            vm.CreateCommand = new Command((parameter) => 
            {
                try
                {
                    new CSharpProjectCreator().Create(block, vm);
                }
                catch (Exception ex)
                {
                    Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }

                view.Close();
            },
            (parameter) => true);

            vm.CancelCommand = new Command((parameter) =>
            {
                view.Close();
            },
            (parameter) => true);

            view.DataContext = vm;
            view.ShowDialog();
        }

        private void ImportBlocksFromCode()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                ImportBlocksFromCode(dlg.FileNames);
            }
        }

        private void ImportBlocksFromCode(string[] paths)
        {
            try
            {
                foreach (var path in paths)
                {
                    using (var fs = System.IO.File.OpenText(path))
                    {
                        var csharp = fs.ReadToEnd();
                        if (!string.IsNullOrEmpty(csharp))
                        {
                            ImportBlocks(csharp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void ImportBlocks(string csharp)
        {
            var part = new BlockPart() { Blocks = new List<XBlock>() };
            bool result = CSharpCodeImporter.Import<XBlock>(csharp, part);
            if (result == true)
            {
                foreach (var block in part.Blocks)
                {
                    Model.Blocks.Add(block);
                }
            }
        }

        private XBlock OpenBlock(string path)
        {
            try
            {
                using (var fs = System.IO.File.OpenText(path))
                {
                    var json = fs.ReadToEnd();
                    var block = _serializer.Deserialize<XBlock>(json);
                    return block;
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        private void ExportBlock()
        {
            var block = _layers.Editor.CreateBlockFromSelected("Block");
            if (block != null)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "Json (*.json)|*.json",
                    FileName = "block"
                };

                if (dlg.ShowDialog(this) == true)
                {
                    var path = dlg.FileName;
                    SaveBlock(block, path);
                    System.Diagnostics.Process.Start("notepad", path);
                }
            }
        }

        private void SaveBlock(XBlock block, string path)
        {
            try
            {
                var json = _serializer.Serialize(block);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                };
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        #endregion

        #region Template

        private void ApplyTemplate(ITemplate template, IRenderer renderer)
        {
            pageView.Width = template.Width;
            pageView.Height = template.Height;

            pageView.gridView.Container = template.Grid;
            pageView.tableView.Container = template.Table;
            pageView.frameView.Container = template.Frame;

            pageView.gridView.Renderer = renderer;
            pageView.tableView.Renderer = renderer;
            pageView.frameView.Renderer = renderer;

            _template = template;

            pageView.gridView.InvalidateVisual();
            pageView.tableView.InvalidateVisual();
            pageView.frameView.InvalidateVisual();
        }

        private void ImportTemplate()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = OpenTemplate(dlg.FileName);
                if (template != null)
                {
                    Model.Templates.Add(template);
                }
            }
        }

        private void ImportTemplatesFromCode()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                ImportTemplatesFromCode(dlg.FileNames);
            }
        }

        private void ImportTemplatesFromCode(string[] paths)
        {
            try
            {
                foreach (var path in paths)
                {
                    using (var fs = System.IO.File.OpenText(path))
                    {
                        var csharp = fs.ReadToEnd();
                        if (!string.IsNullOrEmpty(csharp))
                        {
                            ImportTemplates(csharp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void ImportTemplates(string csharp)
        {
            var part = new TemplatePart() { Templates = new List<ITemplate>() };
            bool result = CSharpCodeImporter.Import<ITemplate>(csharp, part);
            if (result == true)
            {
                foreach (var template in part.Templates)
                {
                    Model.Templates.Add(template);
                }
            }
        }

        private ITemplate OpenTemplate(string path)
        {
            try
            {
                using (var fs = System.IO.File.OpenText(path))
                {
                    var json = fs.ReadToEnd();
                    var template = _serializer.Deserialize<XTemplate>(json);
                    return template;
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        private void ExportTemplate()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Json (*.json)|*.json",
                FileName = _template.Name
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = CreateTemplate(_template);
                var path = dlg.FileName;
                SaveTemplate(path, template);
                System.Diagnostics.Process.Start("notepad", path);
            }
        }

        private XTemplate CreateTemplate(ITemplate template)
        {
            return new XTemplate()
            {
                Width = template.Width,
                Height = template.Height,
                Name = template.Name,
                Grid = new XContainer()
                {
                    Styles = new List<IStyle>(template.Grid.Styles),
                    Shapes = new List<IShape>(template.Grid.Shapes)
                },
                Table = new XContainer()
                {
                    Styles = new List<IStyle>(template.Table.Styles),
                    Shapes = new List<IShape>(template.Table.Shapes)
                },
                Frame = new XContainer()
                {
                    Styles = new List<IStyle>(template.Frame.Styles),
                    Shapes = new List<IShape>(template.Frame.Shapes)
                }
            };
        }

        private void SaveTemplate(string path, ITemplate template)
        {
            try
            {
                var json = _serializer.Serialize(template);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        #endregion

        #region Simulation Overlay

        private void InitSimulationOverlay(IDictionary<XBlock, BoolSimulation> simulations)
        {
            _layers.Editor.SelectionReset();

            _layers.Overlay.EnableSimulationCache = true;
            _layers.Overlay.HaveSimulationCache = false;

            foreach (var simulation in simulations)
            {
                _layers.Blocks.Hidden.Add(simulation.Key);
                _layers.Overlay.Shapes.Add(simulation.Key);
            }

            _layers.Editor.Simulations = simulations;
            _layers.Overlay.Simulations = simulations;

            _layers.Blocks.InvalidateVisual();
            _layers.Overlay.InvalidateVisual();
        }

        private void ResetSimulationOverlay()
        {
            _layers.Editor.Simulations = null;
            _layers.Overlay.Simulations = null;

            _layers.Overlay.HaveSimulationCache = false;

            _layers.Blocks.Hidden.Clear();
            _layers.Overlay.Shapes.Clear();
            _layers.Blocks.InvalidateVisual();
            _layers.Overlay.InvalidateVisual();
        }

        #endregion

        #region Simulation Graph

        private void Graph()
        {
            try
            {
                XPage temp = _layers.Editor.Layers.ToPage("Page", null);
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        SaveGraph(context);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void SaveGraph(PageGraphContext context)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Graph (*.txt)|*.txt",
                FileName = "graph"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var path = dlg.FileName;
                SaveGraph(path, context);
                System.Diagnostics.Process.Start("notepad", path);
            }
        }

        private void SaveGraph(string path, PageGraphContext context)
        {
            using (var writer = new System.IO.StringWriter())
            {
                PageGraphDebug.WriteConnections(context, writer);
                PageGraphDebug.WriteDependencies(context, writer);
                PageGraphDebug.WritePinTypes(context, writer);
                PageGraphDebug.WriteOrderedBlocks(context, writer);

                string text = writer.ToString();
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(text);
                };
            }
        }

        #endregion

        #region Simulation Mode

        private bool IsSimulationRunning()
        {
            return _timer != null;
        }

        private void Start(IDictionary<XBlock, BoolSimulation> simulations)
        {
            _clock = new Clock(cycle: 0L, resolution: 100);

            _timer = new System.Threading.Timer(
                (state) =>
                {
                    try
                    {
                        BoolSimulationFactory.Run(simulations, _clock);
                        _clock.Tick();
                        Dispatcher.Invoke(() => _layers.Overlay.InvalidateVisual());
                    }
                    catch (Exception ex)
                    {
                        Log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);

                        Dispatcher.Invoke(() =>
                        {
                            if (IsSimulationRunning())
                            {
                                Stop();
                            }
                        });
                    }
                }, 
                null, 0, _clock.Resolution);
        }

        private void Start()
        {
            try
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                XPage temp = _layers.Editor.Layers.ToPage("Page", null);
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        var simulations = BoolSimulationFactory.Create(context);
                        if (simulations != null)
                        {
                            SetToolSelection();
                            InitSimulationOverlay(simulations);
                            Start(simulations);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void Restart()
        {
            Stop();
            Start();
        }

        private void Stop()
        {
            try
            {
                ResetSimulationOverlay();

                if (IsSimulationRunning())
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void Options()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}