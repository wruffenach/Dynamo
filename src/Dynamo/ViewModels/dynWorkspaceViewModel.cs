﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo
{
    class dynWorkspaceViewModel: dynViewModelBase
    {
        public dynWorkspace _workspace;

        ObservableCollection<dynConnectorViewModel> _connectors = new ObservableCollection<dynConnectorViewModel>();
        ObservableCollection<dynNodeViewModel> _nodes = new ObservableCollection<dynNodeViewModel>();
        ObservableCollection<dynNoteVIewModel> _notes = new ObservableCollection<dynNoteVIewModel>(); 

        public ObservableCollection<dynWorkspaceViewModel> Connectors
        {
            get { return _connectors; }
            set { 
                _connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }
        public ObservableCollection<dynWorkspaceViewModel> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }
        public ObservableCollection<dynWorkspaceViewModel> Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
            }
        }

        public DelegateCommand<object> CreateNodeCommand { get; set; }
        public DelegateCommand<object> CreateConnectionCommand { get; set; }
        public DelegateCommand<string> AddNoteCommand { get; set; }
        public DelegateCommand<object> DeleteCommand { get; set; }
        public DelegateCommand NodeFromSelectionCommand { get; set; }

        public string Name
        {
            get{return Workspace.Name}
        }

        public dynWorkspaceViewModel(dynWorkspace workspace)
        {
            _workspace = workspace;
            _workspace.NodeAdded += _workspace_NodeAdded;
            _workspace.ConnectorAdded += _workspace_ConnectorAdded;
            _workspace.NoteAdded += _workspace_NoteAdded;

            CreateNodeCommand = new DelegateCommand<object>(CreateNode, CanCreateNode);
            CreateConnectionCommand = new DelegateCommand<object>(CreateConnection, CanCreateConnection);
            AddNoteCommand = new DelegateCommand<string>(AddNote, CanAddNote);
            DeleteCommand = new DelegateCommand<object>(Delete, CanDelete);
            NodeFromSelectionCommand = new DelegateCommand(CreateNodeFromSelection, CanCreateNodeFromSelection);

            _workspace.PropertyChanged += Workspace_PropertyChanged;
        }

        void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Name")
                RaisePropertyChanged("Name");
        }

        private void CreateNodeFromSelection()
        {
            DynamoModel.Instance.CollapseNodes(
                DynamoSelection.Instance.Selection.Where(x => x is dynNodeViewModel)
                    .Select(x => (x as dynNodeViewModel).NodeLogic));
            
        }

        private bool CanCreateNodeFromSelection()
        {
            if (DynamoSelection.Instance.Selection.Count > 0)
            {
                return true;  
            }
        }

        void _workspace_NoteAdded(object sender, EventArgs e)
        {
            Notes.Add(new dynNoteModelView(sender as dynNote));
        }

        void _workspace_ConnectorAdded(object sender, EventArgs e)
        {
            Connectors.Add(new dynControllerModelView(sender as dynNode));
        }

        void _workspace_NodeAdded(object sender, EventArgs e)
        {
            Nodes.Add(new dynNodeModelView(sender as dynNode));
        }

        void CreateNode(object parameters)
        {
            Dictionary<string, object> data = parameters as Dictionary<string, object>;
            if (data == null)
            {
                return;
            }

            dynNode node = dynSettings.Controller.CreateNode(data["name"].ToString());

            dynNodeUI nodeUi = node.NodeUI;
            if (dynSettings.Workbench != null)
            {
                dynSettings.Workbench.Children.Add(nodeUi);
            }

            dynSettings.Controller.Nodes.Add(NodeLogic);
            NodeLogic.WorkSpace = dynSettings.Controller.CurrentSpace;
            nodeUi.Opacity = 1;

            //if we've received a value in the dictionary
            //try to set the value on the node
            if (data.ContainsKey("value"))
            {
                if (typeof(dynBasicInteractive<double>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<double>).Value = (double)data["value"];
                }
                else if (typeof(dynBasicInteractive<string>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<string>).Value = data["value"].ToString();
                }
                else if (typeof(dynBasicInteractive<bool>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<bool>).Value = (bool)data["value"];
                }
                else if (typeof(dynVariableInput).IsAssignableFrom(node.GetType()))
                {
                    int desiredPortCount = (int)data["value"];
                    if (node.InPortData.Count < desiredPortCount)
                    {
                        int portsToCreate = desiredPortCount - node.InPortData.Count;

                        for (int i = 0; i < portsToCreate; i++)
                        {
                            (node as dynVariableInput).AddInput();
                        }
                        (node as dynVariableInput).RegisterAllPorts();
                    }
                }
            }

            //override the guid so we can store
            //for connection lookup
            if (data.ContainsKey("guid"))
            {
                GUID = (Guid)data["guid"];
            }
            else
            {
                GUID = Guid.NewGuid();
            }

            // by default place node at center
            var x = 0.0;
            var y = 0.0;
            if (dynSettings.Bench != null)
            {
                x = dynSettings.Bench.outerCanvas.ActualWidth / 2.0;
                y = dynSettings.Bench.outerCanvas.ActualHeight / 2.0;

                // apply small perturbation
                // so node isn't right on top of last placed node
                Random r = new Random();
                x += (r.NextDouble() - 0.5) * 50;
                y += (r.NextDouble() - 0.5) * 50;
            }

            var transformFromOuterCanvas = data.ContainsKey("transformFromOuterCanvasCoordinates");

            if (data.ContainsKey("x"))
                x = (double)data["x"];

            if (data.ContainsKey("y"))
                y = (double)data["y"];

            Point dropPt = new Point(x, y);

            // Transform dropPt from outerCanvas space into zoomCanvas space
            if (transformFromOuterCanvas)
            {
                var a = dynSettings.Bench.outerCanvas.TransformToDescendant(dynSettings.Bench.WorkBench);
                dropPt = a.Transform(dropPt);
            }

            // center the node at the drop point
            if (!Double.IsNaN(nodeUi.ActualWidth))
                dropPt.X -= (nodeUi.ActualWidth / 2.0);

            if (!Double.IsNaN(nodeUi.ActualHeight))
                dropPt.Y -= (nodeUi.ActualHeight / 2.0);

            Canvas.SetLeft(nodeUi, dropPt.X);
            Canvas.SetTop(nodeUi, dropPt.Y);

            nodeUi.EnableInteraction();

            if (dynSettings.Controller.ViewingHomespace)
            {
                NodeLogic.SaveResult = true;
            }
        }

        bool CanCreateNode(object parameters)
        {
            Dictionary<string, object> data = parameters as Dictionary<string, object>;

            if (data != null &&
                (dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(data["name"].ToString()) ||
                //dynSettings.Controller.CustomNodeLoader.Contains( Guid.Parse( data["name"].ToString() ) ) ||
                    dynSettings.FunctionDict.ContainsKey(Guid.Parse((string)data["name"]))))
            {
                return true;
            }

            return false;
        }

        void CreateConnection(object parameters)
        {
            Dictionary<string, object> connectionData = parameters as Dictionary<string, object>;

            dynNodeUI start = (dynNodeUI)connectionData["start"];
            dynNodeUI end = (dynNodeUI)connectionData["end"];
            int startIndex = (int)connectionData["port_start"];
            int endIndex = (int)connectionData["port_end"];

            dynConnector c = new dynConnector(start, end, startIndex, endIndex, 0);

            dynSettings.Controller.CurrentSpace.Connectors.Add(c);
        }

        bool CanCreateConnection(object parameters)
        {
            //make sure you have valid connection data
            Dictionary<string, object> connectionData = parameters as Dictionary<string, object>;
            if (connectionData != null && connectionData.Count == 4)
            {
                return true;
            }

            return false;
        }

        private void Delete(object parameters)
        {
            //if you get an object in the parameters, just delete that object
            if (parameters != null)
            {
                dynNote note = parameters as dynNote;
                dynNodeUI node = parameters as dynNodeUI;

                if (node != null)
                {
                    DeleteNode(node);
                }
                else if (note != null)
                {
                    DeleteNote(note);
                }
            }
            else
            {
                for (int i = dynSettings.Workbench.Selection.Count - 1; i >= 0; i--)
                {
                    dynNote note = dynSettings.Workbench.Selection[i] as dynNote;
                    dynNodeUI node = dynSettings.Workbench.Selection[i] as dynNodeUI;

                    if (node != null)
                    {
                        DeleteNode(node);
                    }
                    else if (note != null)
                    {
                        DeleteNote(note);
                    }
                }
            }
        }

        private bool CanDelete()
        {
            return dynSettings.Workbench.Selection.Count > 0;
        }

        private void AddNote(object parameters)
        {
            Dictionary<string, object> inputs = (Dictionary<string, object>)parameters;

            dynNoteModel n = new dynNoteModel((double)inputs["x"], (double)inputs["y"]);

            n.Text = inputs["text"].ToString();
            dynWorkspace ws = (dynWorkspace)inputs["workspace"];

            ws.Notes.Add(n);
            dynSettings.Bench.WorkBench.Children.Add(n);

            if (_workspace.Model.CurrentSpace != _workspace.Model.HomeSpace)
            {
                _workspace.Model.CurrentSpace.Modified();
            }
        }

        private bool CanAddNote(object parameters)
        {
            return true;
        }

        private void DeleteNote(dynNote note)
        {
            DynamoSelection.Instance.Selection.Remove(note);
            _workspace.Model.CurrentSpace.Notes.Remove(note);
            dynSettings.Workbench.Children.Remove(note);
        }

        private static void DeleteNode(dynNodeViewModel node)
        {
            foreach (var port in node.OutPorts)
            {
                for (int j = port.Connectors.Count - 1; j >= 0; j--)
                {
                    port.Connectors[j].Kill();
                }
            }

            foreach (dynPort p in node.InPorts)
            {
                for (int j = p.Connectors.Count - 1; j >= 0; j--)
                {
                    p.Connectors[j].Kill();
                }
            }

            node.NodeLogic.Cleanup();
            dynSettings.Workbench.Selection.Remove(node);
            dynSettings.Controller.Nodes.Remove(node.NodeLogic);
            dynSettings.Workbench.Children.Remove(node);
        }
    }
}