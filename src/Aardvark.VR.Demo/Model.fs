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
type Polygon = 
    { 
        vertices : plist<V3d>
    }

[<DomainType>]
type FinishedLine = 
    {
        finishedLineOnMars      : plist<VisibleSphere>
        finishedLineMarsDisplay : Line3d[]

    }

[<DomainType>]
type Model =
    {
        text    : string
        vr      : bool                
        
        cameraState         : CameraControllerState

        ControllerPosition  : V3d
        offsetToCenter      : V3d
        
        controllerInfos          : hmap<ControllerKind, ControllerInfo> // CURRENT CONTROLLER INFO!
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


        initWorkSpaceTrafo          : Trafo3d // describes all accumulated drag and rotation actions by the user (how the opc and everything is moved)
                                              // END -> update initWorkSpaceTrafo by workSpaceTrafo for the next iteration...
        workSpaceTrafo              : Trafo3d // START and MOVE -> initWorkSpaceTrafo * controller-DELTA
        opcSpaceTrafo               : Trafo3d //  description of how the opc is moved from 25k to our worldspace-origin (controller space)
        annotationSpaceTrafo        : Trafo3d // (identity) lives at the origin...but later for accuracy reasons...model trafo like opcSpace...
        initOpcSpaceTrafo           : Trafo3d // STATIC opcSpace at Start-CLICK
        initAnnotationSpaceTrafo    : Trafo3d // STATIC annotationSpace at Start-CLICK
        
        initControlTrafo            : Trafo3d // START Controller1 (temporal!)
        init2ControlTrafo           : Trafo3d // START Controller2 (temporal!)

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
        finishedLine                : hmap<string, FinishedLine>
        
    }
