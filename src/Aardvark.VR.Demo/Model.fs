namespace Demo.Main

open Aardvark.Vr
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives
open Aardvark.SceneGraph.Opc
open OpcViewer.Base.Picking
open OpcViewer.Base.Attributes
open Aardvark.Rendering.Vulkan
open Demo.Menu
open Demo

[<DomainType>]
type Line = {
    line : Line3d
    color : C4b
}

type State =
    | Pressed
    | Released
    | Hold

type ButtonStates = {
    front : State
    back : State
}

[<DomainType>]
type Polygon = { vertices : V3d[]; }

[<DomainType>]
type Model =
    {
        text    : string
        vr      : bool                
        
        cameraState         : CameraControllerState

        ControllerPosition  : V3d
        offsetToCenter      : V3d
        
        controllerInfos          : hmap<ControllerKind, ControllerInfo>
        offsetControllerDistance : float

        //OPC model part
        [<NonIncremental>]
        patchHierarchies            : list<PatchHierarchy> 
        boundingBox                 : Box3d
        rotationAxis                : Trafo3d
        opcInfos                    : hmap<Box3d, OpcData>
        opcAttributes               : AttributeModel
        mainFrustum                 : Frustum
        rotateBox                   : bool
        pickingModel                : PickingModel

        globalTrafo                 : Trafo3d
        controllerGlobalTrafo       : Trafo3d
        initControllerGlobalTrafo   : Trafo3d
        initGlobalTrafo             : Trafo3d
        initControlTrafo            : Trafo3d
        init2ControlTrafo           : Trafo3d

        menuModel                   : MenuModel
        //Annotation tools
        drawingPoint                : plist<VisibleBox> 
        drawingLine                 : Line3d[]

        currentlyDrawing            : Option<Polygon>
        finishedDrawings            : hmap<string, Polygon>

        flagOnController            : plist<VisibleBox> 
        flagOnMars                  : plist<VisibleBox> 
        lineOnController            : plist<VisibleSphere>
        lineOnMars                  : plist<VisibleSphere>
        lineMarsDisplay             : Line3d[]
        lineDistance                : float


        pressGlobalTrafo    : Trafo3d
        unpressGlobalTrafo  : Trafo3d

    }
