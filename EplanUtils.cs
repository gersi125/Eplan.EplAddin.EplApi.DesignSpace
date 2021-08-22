using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.EObjects;
using Eplan.EplApi.HEServices;
using Cloud = at.jku.sea.cloud.rest.client.dotnet.CloudInterfaces;
using JKU.ISSE.DesignSpace.SDK.Domain.Model;


namespace Eplan.EplAddin.EplApi.DesignSpace
{
    class EplanUtils
    {
        class EplanUtils
        {
            private Cloud.Workspace workspace;
            Cloud.Artifact connectionType;
            Cloud.Artifact functionType;
            Cloud.Package package;
            public EplanUtils(Cloud.Workspace workspace_)
            {
                workspace = workspace_;
                functionType = workspace.getArtifact(Fields.EPLAN_FUNCTION_TYPE);
                connectionType = workspace.getArtifact(Fields.EPLAN_CONNECTION_TYPE);

                IEnumerable<Cloud.Package> pkgs = workspace.getPackages();
                List<Cloud.Package> tmp = new List<Cloud.Package>();
                foreach (Cloud.Package pkg in pkgs)
                {
                    if (pkg.getPropertyValue(Fields.PKG_NAME).Equals(workspace.getIdentifier()))
                    {
                        tmp.Add(pkg);
                    }
                }
                if (tmp.Count() < 1)
                {
                    package = workspace.createPackage();
                    package.setPropertyValue(workspace, "name", workspace.getIdentifier());
                }
                else
                {
                    package = tmp[0];
                }

                //IEnumerable<Cloud.Artifact> pkgs = workspace.getArtifactsWithProperty("name", workspace.getIdentifier());
                //if (pkgs.Count() < 1)
                //{
                //    package = workspace.createPackage();
                //    package.setPropertyValue(workspace, "name", workspace.getIdentifier());
                //}
                //else
                //{
                //    package = (at.jku.sea.cloud.rest.client.dotnet.Rest.RestPackage)pkgs.First();
                //}

            }


            public Cloud.Workspace GetWorkspace()
            {
                return workspace;
            }

            /// <summary>
            /// Creates Function Artifact on Designspace Server
            /// </summary>
            /// <param name="f">Function that should be created</param>
            /// <returns></returns>
            public Cloud.Artifact CreateFunctionArtifact(Function f)
            {
                Cloud.Artifact artifact = workspace.createArtifact(functionType, package);
                Cloud.Property DTProperty = artifact.createProperty(workspace, Fields.DT);
                DTProperty.setValue(workspace, f.Properties.FUNC_DEVICETAG_FULLNAME.ToString());
                Cloud.Property NameProperty = artifact.createProperty(workspace, Fields.NAME);
                NameProperty.setValue(workspace, FunctionUtils.GetFunctionName(f));
                return artifact;

            }

            /// <summary>
            /// Deletes multiple Artifacts in Designspace
            /// </summary>
            /// <param name="deletedArtifacts">Artifacts that should be deleted</param>
            public void DeleteArtifacts(IEnumerable<Cloud.Artifact> deletedArtifacts)
            {
                foreach (Cloud.Artifact a in deletedArtifacts)
                {
                    a.delete(workspace);
                }
            }

            /// <summary>
            /// Creates multiple Artifacts in Designspace
            /// </summary>
            /// <param name="functions"></param>
            public void CreateFunctionArtifacts(List<Function> functions)
            {
                foreach (Function f in functions)
                {
                    CreateFunctionArtifact(f);
                }
            }

            /// <summary>
            /// Checks if Connections have changed
            /// Not in/of use at the moment
            /// </summary>
            /// <param name="function"></param>
            public void CheckForFunctionChanges(Function function)
            {
                //Not finished
                if (FunctionUtils.IsAnnotated(function))
                {
                    List<Cloud.Artifact> functionArtifacts = workspace.getArtifactsWithProperty(Fields.DT, function.Properties.FUNC_DEVICETAG_FULLNAME, functionType).ToList<Cloud.Artifact>();
                    if (functionArtifacts.Count > 1)
                    {
                        throw new Exception("More than one Artifact with this DT");
                    }

                    List<Connection> connections = FunctionUtils.GetConnections(function);
                    List<Cloud.Artifact> outConnectionArtifacts = workspace.getArtifactsWithProperty(Fields.OUT, function.Properties.FUNC_DEVICETAG_FULLNAME, connectionType).ToList<Cloud.Artifact>();
                    List<Cloud.Artifact> inConnectionArtifacts = workspace.getArtifactsWithProperty(Fields.IN, function.Properties.FUNC_DEVICETAG_FULLNAME, connectionType).ToList<Cloud.Artifact>();

                    List<Connection> outTempList = ConnectionUtils.CalcTempConnectionList(outConnectionArtifacts, connections);
                    List<Connection> inTempList = ConnectionUtils.CalcTempConnectionList(inConnectionArtifacts, connections);
                }
            }



            /// <summary>
            /// Checks if annotated functions were added or deleted and manage connectionn
            /// </summary>
            /// <param name="pr"></param>
            public void CheckForGlobalChanges(Project pr, String eventString, IEnumerable<Function> selectedFunctions)
            {
                Debug.WriteLine("#### Project: " + pr.ProjectName + " -- WSName: " + workspace.getIdentifier());

                IEnumerable<Cloud.Artifact> functionArtifacts = workspace.getArtifacts(functionType); //+ package; getArtifacts(Type) noch nicht implementiert (server wie client seitig)
                List<Cloud.Artifact> artifactList = functionArtifacts.ToList<Cloud.Artifact>();
                List<Function> functionList = FunctionUtils.GetAnnotatedFunctions(pr);

                List<Function> tempList = FunctionUtils.CalcTempFunctionList(artifactList, functionList);

                // These Artifacts should be deleted in Designspace
                List<Cloud.Artifact> deletedFunctions = FunctionUtils.GetDeletedFunctionsAsArtifacts(artifactList, tempList);
                DeleteArtifacts(deletedFunctions);

                //// These Functions should be added in Designspace
                List<Function> addedFunctions = FunctionUtils.GetAddedArtifactsAsFunctions(functionList, tempList);
                CreateFunctionArtifacts(addedFunctions);

                //Update Function Names TODO
                IEnumerable<Function> updateFunctions = functionList.Except<Function>(addedFunctions);
                UpdateFunctions(updateFunctions);


                //Manage Connections
                IEnumerable<Cloud.Artifact> connectionArtifacts = workspace.getArtifacts(connectionType);
                List<Cloud.Artifact> connectionArtifactList = connectionArtifacts.ToList<Cloud.Artifact>();
                List<Connection> connectionList = ConnectionUtils.GetAnnotatedConnections(pr);

                //tempList => welche connections sind sowohl auf Server wie Client existent/gleich
                List<Connection> tempConnList = ConnectionUtils.CalcTempConnectionList(connectionArtifactList, connectionList);

                //Debug.WriteLine("#Connections: " + connectionList.Count);

                IEnumerable<Cloud.Artifact> deletedConnections = ConnectionUtils.GetDeletedConnectionsAsArtifacts(connectionArtifactList, tempConnList);
                UpdateConnections(deletedConnections, selectedFunctions);


                //DeleteArtifacts(deletedConnections);


                HashSet<Tuple<Function, Function>> existingConnectedFunctions = ConnectedFunctions(functionList);

                List<Connection> addedConnections = ConnectionUtils.GetAddedConnectionsAsFunctions(connectionList, tempConnList);
                CreateConnections(addedConnections);

            }

            private HashSet<Tuple<Function, Function>> ConnectedFunctions(List<Function> functionList)
            {
                HashSet<Tuple<Function, Function>> existingConnectedFunctions = new HashSet<Tuple<Function, Function>>();
                foreach (Function f in functionList)
                {
                    List<Function> x = new List<Function>();
                    x.Add(f);
                    foreach (Function f1 in functionList.Except(x))
                    {
                        //f.Pins[0].ConnectionPoint.Direction Up/Down, nur connection zwischen up und down -> down = out, up = in
                        // zusätzlich noch überprüfung ob "dazwischen" sich noch eine Function befindet...

                        //Funktioniert auch nur, wenn nur 1 ZENTRIERTER Pin existiert... (wahrscheinlich)
                        if (f.Location.X == f1.Location.X && f.Page == f1.Page)
                        {
                            foreach (Pin outPin in f.Pins)
                            {
                                if (outPin.Direction == PinBase.Directions.Down)
                                {
                                    foreach (Pin inPin in f1.Pins)
                                    {
                                        if (inPin.Direction == PinBase.Directions.Up)
                                        {

                                        }
                                    }
                                }
                            }
                            Tuple<Function, Function> conn = Tuple.Create(f, f1);
                            existingConnectedFunctions.Add(conn);
                            Debug.WriteLine("YEHA!");
                            Debug.WriteLine("F X " + f.Location.X);
                            Debug.WriteLine("F1 X " + f1.Location.X);
                        }
                    }
                }
                return existingConnectedFunctions;
            }


            private void UpdateConnections(IEnumerable<Cloud.Artifact> deletedConnections, IEnumerable<Function> selectedFunctions)
            {
                if (selectedFunctions == null) return;

                foreach (Cloud.Artifact a in deletedConnections)
                {
                    bool fOut = false;
                    bool fIn = false;
                    Cloud.Artifact outArtifact = (Cloud.Artifact)a.getPropertyValue(Fields.OUT);
                    String outPin = null;
                    if (outArtifact != null)
                    {
                        outPin = a.getPropertyValue(Fields.OUT_PIN).ToString();
                        fOut = selectedFunctions.Any<Function>(x => FunctionUtils.GetFunctionDT(x) == outArtifact.getPropertyValue(Fields.DT).ToString());
                    }

                    Cloud.Artifact inArtifact = (Cloud.Artifact)a.getPropertyValue(Fields.IN);
                    String inPin = null;
                    if (inArtifact != null)
                    {
                        fIn = selectedFunctions.Any<Function>(x => FunctionUtils.GetFunctionDT(x) == inArtifact.getPropertyValue(Fields.DT).ToString());
                        inPin = a.getPropertyValue(Fields.IN_PIN).ToString();
                    }

                    //selectedFunctions.Contains<Function>(x => x.getPropertyValue(Fields.DT).Equals(CommonUtils.GetFunctionDT((Function)c.StartSymbolReference)))


                    if (fOut)
                    {
                        UpdateArtifact(a, Fields.OUT, null);
                        UpdateArtifact(a, Fields.OUT_PIN, null);
                    }

                    if (fIn)
                    {
                        UpdateArtifact(a, Fields.IN, null);
                        UpdateArtifact(a, Fields.IN_PIN, null);
                    }

                }
            }

            private void UpdateArtifact(Cloud.Artifact a, String propName, Object propValue)
            {
                a.setPropertyValue(workspace, propName, propValue);
            }



            private void CheckFunctionsOfConnection(List<Connection> connectionList)
            {
                foreach (Connection c in connectionList)
                {
                    Function start = (Function)c.StartSymbolReference;
                    Function end = (Function)c.EndSymbolReference;

                    Connection[] c1 = start.Connections;
                    Debug.WriteLine("C1: Conn#: " + c1.Length);
                    Connection[] c2 = end.Connections;
                    Debug.WriteLine("C2: Conn#: " + c2.Length);
                }
            }

            private void UpdateFunctions(IEnumerable<Function> updateFunctions)
            {
                //workspace.getArtifacts/functionArtifacts oder artefakte einzeln holen schneller??
                foreach (Function f in updateFunctions)
                {
                    Cloud.Artifact a = workspace.getArtifactsWithProperty(Fields.DT, FunctionUtils.GetFunctionDT(f)).ElementAt(0);
                    String functionName = FunctionUtils.GetFunctionName(f);
                    if (!functionName.Equals(a.getPropertyValue(Fields.NAME)))
                    {
                        a.setPropertyValue(workspace, Fields.NAME, functionName);
                        Debug.WriteLine("Update functionname");
                    }
                }
            }

            /// <summary>
            /// Creates Connection Artifacts on Designspace Server
            /// </summary>
            /// <param name="addedConnections">Connections that should be created</param>
            private void CreateConnections(List<Connection> addedConnections)
            {
                foreach (Connection c in addedConnections)
                {

                    Cloud.Artifact startArtifact = workspace.getArtifactsWithProperty(Fields.DT, FunctionUtils.GetFunctionDT((Function)c.StartSymbolReference)).ElementAt(0);
                    Cloud.Artifact endArtifact = workspace.getArtifactsWithProperty(Fields.DT, FunctionUtils.GetFunctionDT((Function)c.EndSymbolReference)).ElementAt(0);

                    Dictionary<String, Object> inDic = new Dictionary<string, object>();
                    inDic[Fields.IN] = endArtifact;
                    inDic[Fields.IN_PIN] = c.EndPin.ToString();
                    List<Cloud.Artifact> conWithExistingIn = workspace.getArtifactsWithProperty(inDic, connectionType).ToList<Cloud.Artifact>();


                    Dictionary<String, Object> outDic = new Dictionary<string, object>();
                    outDic[Fields.OUT] = startArtifact;
                    outDic[Fields.OUT_PIN] = c.StartPin.ToString();
                    List<Cloud.Artifact> conWithExistingOut = workspace.getArtifactsWithProperty(outDic, connectionType).ToList<Cloud.Artifact>();


                    //Cloud.Artifact existingCon;

                    if (conWithExistingIn.Count > 0)
                    {
                        foreach (Cloud.Artifact existingCon in conWithExistingIn)
                        {
                            //existingCon = conWithExistingIn[0];
                            existingCon.setPropertyValue(workspace, Fields.OUT, startArtifact);
                            existingCon.setPropertyValue(workspace, Fields.OUT_PIN, c.StartPin.ToString());
                        }
                    }
                    else if (conWithExistingOut.Count > 0)
                    {
                        foreach (Cloud.Artifact existingCon in conWithExistingOut)
                        {
                            //existingCon = conWithExistingOut[0];
                            existingCon.setPropertyValue(workspace, Fields.IN, endArtifact);
                            existingCon.setPropertyValue(workspace, Fields.IN_PIN, c.EndPin.ToString());
                        }
                    }
                    else
                    {
                        Cloud.Artifact connectionArtifact = workspace.createArtifact(connectionType, package);
                        Cloud.Property connOut = connectionArtifact.createProperty(workspace, Fields.OUT);
                        Cloud.Property connOutPin = connectionArtifact.createProperty(workspace, Fields.OUT_PIN);
                        Cloud.Property connIn = connectionArtifact.createProperty(workspace, Fields.IN);
                        Cloud.Property connInPin = connectionArtifact.createProperty(workspace, Fields.IN_PIN);

                        //Cloud.Artifact startArtifact = functionArtifacts.Single<Cloud.Artifact>(x => x.getPropertyValue(Fields.DT).Equals(CommonUtils.GetFunctionDT((Function)c.StartSymbolReference)));
                        //Cloud.Artifact endArtifact = functionArtifacts.Single<Cloud.Artifact>(x => x.getPropertyValue(Fields.DT).Equals(CommonUtils.GetFunctionDT((Function)c.EndSymbolReference)));

                        connOut.setValue(workspace, startArtifact);
                        connOutPin.setValue(workspace, c.StartPin.ToString());
                        connIn.setValue(workspace, endArtifact);
                        connInPin.setValue(workspace, c.EndPin.ToString());
                    }
                }
            }
            public Cloud.Artifact CreateFunctionArtifact(String DT)
            {
                Cloud.Artifact artifact = workspace.createArtifact(functionType, package);
                Cloud.Property DTProperty = artifact.createProperty(workspace, Fields.DT);
                DTProperty.setValue(workspace, DT);
                return artifact;
            }

            public void CreateConnection(Pin endPin, Pin startPin, Cloud.Artifact startArtifact, Cloud.Artifact endArtifact)
            {
                if (startArtifact != null && endArtifact != null)
                {
                    Dictionary<String, Object> propertyMap = new Dictionary<string, object>();
                    propertyMap.Add(Fields.OUT, startArtifact);
                    propertyMap.Add(Fields.OUT_PIN, startPin);
                    propertyMap.Add(Fields.IN, endArtifact);
                    propertyMap.Add(Fields.IN_PIN, endPin);
                    IEnumerable<Cloud.Artifact> connections = workspace.getArtifactsWithProperty(propertyMap, connectionType);

                    List<Cloud.Artifact> connectionsList = connections.ToList<Cloud.Artifact>();
                    if (connectionsList.Count < 1)
                    {
                        Cloud.Artifact connectionArtifact = workspace.createArtifact(connectionType);
                        Cloud.Property connOut = connectionArtifact.createProperty(workspace, Fields.OUT);
                        Cloud.Property connOutPin = connectionArtifact.createProperty(workspace, Fields.OUT_PIN);
                        Cloud.Property connIn = connectionArtifact.createProperty(workspace, Fields.IN);
                        Cloud.Property connInPin = connectionArtifact.createProperty(workspace, Fields.IN_PIN);

                        connOut.setValue(workspace, startArtifact);
                        connOutPin.setValue(workspace, startPin.ToString());
                        connIn.setValue(workspace, endArtifact);
                        connInPin.setValue(workspace, endPin.ToString());
                    }
                }
            }



        }
    }
}
