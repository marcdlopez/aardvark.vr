namespace Demo 

open System.IO
open Aardvark.SceneGraph
open Aardvark.SceneGraph.Opc
open Aardvark.Base
open OpcViewer.Base
open OpcViewer.Base.Picking
open Aardvark.VRVis.Opc
open Aardvark.UI.Primitives


module OpcViewerFunc =
    let patchHierarchiesImport dir =
        [ 
            for h in Directory.GetDirectories(dir) |> Array.head |> Array.singleton do
                yield PatchHierarchy.load SerializationOpc.binarySerializer.Pickle SerializationOpc.binarySerializer.UnPickle (h |> OpcPaths)
        ]

    let boxImport patchHierarchies  = 
        patchHierarchies
            |> List.map(fun x -> x.tree |> QTree.getRoot) 
            |> List.map(fun x -> x.info.GlobalBoundingBox)
            |> List.fold (fun a b -> Box3d.Union(a, b)) Box3d.Invalid

    let opcInfosImport patchHierarchies= 
        [
            for h in patchHierarchies do
                
                let rootTree = h.tree |> QTree.getRoot
    
                yield {
                    patchHierarchy = h
                    kdTree         = Aardvark.VRVis.Opc.KdTrees.expandKdTreePaths h.opcPaths.Opc_DirAbsPath (KdTrees.loadKdTrees' h Trafo3d.Identity true ViewerModality.XYZ SerializationOpc.binarySerializer)
                    localBB        = rootTree.info.LocalBoundingBox 
                    globalBB       = rootTree.info.GlobalBoundingBox
                    neighborMap    = HMap.empty
                }
        ]
        |> List.map (fun info -> info.globalBB, info)
        |> HMap.ofList

    let getUpVector (box : Box3d) (rotate : bool)= 
      if rotate then (box.Center.Normalized) else V3d.OOI
    
    let restoreCamStateImport (box : Box3d) up: CameraControllerState =
        //if File.Exists ".\camstate" then          
        //    Log.line "[App] restoring camstate"
        //    let csLight : CameraStateLean = Serialization.loadAs ".\camstate"
        //    { FreeFlyController.initial with view = csLight |> fromCameraStateLean }
        //else 
            { FreeFlyController.initial with view = CameraView.lookAt (box.Max) box.Center up; }