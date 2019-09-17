namespace OpcViewer.Base.Attributes

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open OpcViewer.Base.Attributes

[<AutoOpen>]
module Mutable =

    
    
    type MScalarLayer(__initial : OpcViewer.Base.Attributes.ScalarLayer) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Attributes.ScalarLayer> = Aardvark.Base.Incremental.EqModRef<OpcViewer.Base.Attributes.ScalarLayer>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Attributes.ScalarLayer>
        let _label = ResetMod.Create(__initial.label)
        let _actualRange = ResetMod.Create(__initial.actualRange)
        let _definedRange = ResetMod.Create(__initial.definedRange)
        let _index = ResetMod.Create(__initial.index)
        let _colorLegend = OpcViewer.Base.FalseColors.Mutable.MFalseColorsModel.Create(__initial.colorLegend)
        
        member x.label = _label :> IMod<_>
        member x.actualRange = _actualRange :> IMod<_>
        member x.definedRange = _definedRange :> IMod<_>
        member x.index = _index :> IMod<_>
        member x.colorLegend = _colorLegend
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcViewer.Base.Attributes.ScalarLayer) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_label,v.label)
                ResetMod.Update(_actualRange,v.actualRange)
                ResetMod.Update(_definedRange,v.definedRange)
                ResetMod.Update(_index,v.index)
                OpcViewer.Base.FalseColors.Mutable.MFalseColorsModel.Update(_colorLegend, v.colorLegend)
                
        
        static member Create(__initial : OpcViewer.Base.Attributes.ScalarLayer) : MScalarLayer = MScalarLayer(__initial)
        static member Update(m : MScalarLayer, v : OpcViewer.Base.Attributes.ScalarLayer) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcViewer.Base.Attributes.ScalarLayer> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module ScalarLayer =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let label =
                { new Lens<OpcViewer.Base.Attributes.ScalarLayer, System.String>() with
                    override x.Get(r) = r.label
                    override x.Set(r,v) = { r with label = v }
                    override x.Update(r,f) = { r with label = f r.label }
                }
            let actualRange =
                { new Lens<OpcViewer.Base.Attributes.ScalarLayer, Aardvark.Base.Range1d>() with
                    override x.Get(r) = r.actualRange
                    override x.Set(r,v) = { r with actualRange = v }
                    override x.Update(r,f) = { r with actualRange = f r.actualRange }
                }
            let definedRange =
                { new Lens<OpcViewer.Base.Attributes.ScalarLayer, Aardvark.Base.Range1d>() with
                    override x.Get(r) = r.definedRange
                    override x.Set(r,v) = { r with definedRange = v }
                    override x.Update(r,f) = { r with definedRange = f r.definedRange }
                }
            let index =
                { new Lens<OpcViewer.Base.Attributes.ScalarLayer, System.Int32>() with
                    override x.Get(r) = r.index
                    override x.Set(r,v) = { r with index = v }
                    override x.Update(r,f) = { r with index = f r.index }
                }
            let colorLegend =
                { new Lens<OpcViewer.Base.Attributes.ScalarLayer, OpcViewer.Base.FalseColors.FalseColorsModel>() with
                    override x.Get(r) = r.colorLegend
                    override x.Set(r,v) = { r with colorLegend = v }
                    override x.Update(r,f) = { r with colorLegend = f r.colorLegend }
                }
    
    
    type MAttributeModel(__initial : OpcViewer.Base.Attributes.AttributeModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Attributes.AttributeModel> = Aardvark.Base.Incremental.EqModRef<OpcViewer.Base.Attributes.AttributeModel>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Attributes.AttributeModel>
        let _scalarLayers = MMap.Create(__initial.scalarLayers, (fun v -> MScalarLayer.Create(v)), (fun (m,v) -> MScalarLayer.Update(m, v)), (fun v -> v))
        let _selectedScalar = MOption.Create(__initial.selectedScalar, (fun v -> MScalarLayer.Create(v)), (fun (m,v) -> MScalarLayer.Update(m, v)), (fun v -> v))
        let _textureLayers = MList.Create(__initial.textureLayers)
        
        member x.scalarLayers = _scalarLayers :> amap<_,_>
        member x.selectedScalar = _selectedScalar :> IMod<_>
        member x.textureLayers = _textureLayers :> alist<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcViewer.Base.Attributes.AttributeModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_scalarLayers, v.scalarLayers)
                MOption.Update(_selectedScalar, v.selectedScalar)
                MList.Update(_textureLayers, v.textureLayers)
                
        
        static member Create(__initial : OpcViewer.Base.Attributes.AttributeModel) : MAttributeModel = MAttributeModel(__initial)
        static member Update(m : MAttributeModel, v : OpcViewer.Base.Attributes.AttributeModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcViewer.Base.Attributes.AttributeModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module AttributeModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let scalarLayers =
                { new Lens<OpcViewer.Base.Attributes.AttributeModel, Aardvark.Base.hmap<System.String,OpcViewer.Base.Attributes.ScalarLayer>>() with
                    override x.Get(r) = r.scalarLayers
                    override x.Set(r,v) = { r with scalarLayers = v }
                    override x.Update(r,f) = { r with scalarLayers = f r.scalarLayers }
                }
            let selectedScalar =
                { new Lens<OpcViewer.Base.Attributes.AttributeModel, Microsoft.FSharp.Core.Option<OpcViewer.Base.Attributes.ScalarLayer>>() with
                    override x.Get(r) = r.selectedScalar
                    override x.Set(r,v) = { r with selectedScalar = v }
                    override x.Update(r,f) = { r with selectedScalar = f r.selectedScalar }
                }
            let textureLayers =
                { new Lens<OpcViewer.Base.Attributes.AttributeModel, Aardvark.Base.plist<OpcViewer.Base.Attributes.TextureLayer>>() with
                    override x.Get(r) = r.textureLayers
                    override x.Set(r,v) = { r with textureLayers = v }
                    override x.Update(r,f) = { r with textureLayers = f r.textureLayers }
                }
