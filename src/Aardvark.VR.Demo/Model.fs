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

type LinePoints = 
    {
        pos: V3d
        hovered: bool
        color : C4b
    }

[<DomainType>]
type FinishedLine = 
    {
        points          : plist<LinePoints>
        trafo           : Trafo3d
        colorLine       : C4b
        colorVertices   : C4b
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
        opcSpaceTrafo               : Trafo3d // description of how the opc is moved from 25k to our worldspace-origin (controller space)
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

        flagOnController                : plist<VisibleBox> 
        flagOnAnnotationSpace           : plist<VisibleBox> 
        lineOnController                : plist<VisibleSphere>
        lineOnAnnotationSpace           : plist<VisibleSphere>
        lineIsHovered                   : bool
        lineMarsDisplay                 : Line3d[]
        finishedLine                    : hmap<string, FinishedLine>
        dipAndStrikeOnController        : plist<VisibleCylinder>
        dipAndStrikeOnAnnotationSpace   : plist<VisibleCylinder>
        
    }

module Model =
    let initial = 
        let rotateBoxInit = false
        let patchHierarchiesInit = 
            OpcViewerFunc.patchHierarchiesImport "C:\Users\lopez\Desktop\VictoriaCrater\HiRISE_VictoriaCrater_SuperResolution"

        let boundingBoxInit = 
            OpcViewerFunc.boxImport (patchHierarchiesInit)

        let opcInfosInit = 
            OpcViewerFunc.opcInfosImport (patchHierarchiesInit)

        let up =
            OpcViewerFunc.getUpVector boundingBoxInit rotateBoxInit

        let upRotationTrafo = 
            Trafo3d.RotateInto(boundingBoxInit.Center.Normalized, V3d.OOI)

        let cameraStateInit = 
            OpcViewerFunc.restoreCamStateImport boundingBoxInit V3d.OOI
        {
            text                = "some text"
            vr                  = false

            ControllerPosition      = V3d.OOO
            controllerInfos         = HMap.empty
            offsetToCenter          = V3d.One
            cameraState             = cameraStateInit
            
            patchHierarchies    = patchHierarchiesInit
            boundingBox         = boundingBoxInit
            opcInfos            = opcInfosInit
            opcAttributes       = SurfaceAttributes.initModel "C:\Users\lopez\Desktop\VictoriaCrater\HiRISE_VictoriaCrater_SuperResolution"
            mainFrustum         = Frustum.perspective 60.0 0.01 1000.0 1.0
            rotateBox           = rotateBoxInit
            pickingModel        = OpcViewer.Base.Picking.PickingModel.initial

            offsetControllerDistance    = 1.0

            opcSpaceTrafo               = Trafo3d.Translation(-boundingBoxInit.Center) * upRotationTrafo
            workSpaceTrafo              = Trafo3d.Identity
            annotationSpaceTrafo              = Trafo3d.Identity

            initOpcSpaceTrafo           = Trafo3d.Translation(-boundingBoxInit.Center) * upRotationTrafo
            initWorkSpaceTrafo          = Trafo3d.Identity
            initAnnotationSpaceTrafo          = Trafo3d.Identity

            initControlTrafo            = Trafo3d.Identity
            init2ControlTrafo           = Trafo3d.Identity
            rotationAxis                = Trafo3d.Identity

            menuModel               = Menu.MenuModel.init
            drawingPoint            = PList.empty
            drawingLine             = [|Line3d()|]

            currentlyDrawing        = None
            finishedDrawings        = HMap.empty

            flagOnController        = PList.empty
            flagOnAnnotationSpace   = PList.empty
            lineOnController        = PList.empty
            lineOnAnnotationSpace   = PList.empty
            lineMarsDisplay         = [|Line3d()|]
            finishedLine            = HMap.empty
            lineIsHovered           = false
            dipAndStrikeOnController        = PList.empty
            dipAndStrikeOnAnnotationSpace   = PList.empty
        }

    let initMainReset = 
        {
            initial with 
                opcSpaceTrafo                   = Trafo3d.Translation -initial.boundingBox.Center * Trafo3d.RotateInto(initial.boundingBox.Center.Normalized, V3d.OOI) 
                annotationSpaceTrafo            = Trafo3d.Identity
                workSpaceTrafo                  = Trafo3d.Identity
                flagOnController                = PList.empty
                flagOnAnnotationSpace           = PList.empty
                lineOnController                = PList.empty
                lineOnAnnotationSpace           = PList.empty
                lineMarsDisplay                 = [|Line3d()|]
                drawingLine                     = [|Line3d()|]
                dipAndStrikeOnController        = PList.empty
                dipAndStrikeOnAnnotationSpace   = PList.empty
        }