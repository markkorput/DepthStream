# FuseTools
_A mixed bag of reusable Unity scripts (mostly MonoBehaviour components) for often recurring operations_

## Code documentation
Doxygen documentation: [shortnotion.gitlab.io/fusetools/html/index.html](http://shortnotion.gitlab.io/fusetools/html/index.html)

## Installation
Add the contents of this repository somewhere inside the Assets folder of your Unity project, either as a **static copy** of the repo, **or as a git submodule** (if you intend to make changes/additions to FuseTools and commit them back to this repository).

## The Code
All classes are nested into the ```FuseTools``` namespacea to avoid conflicts with other libraries/packages/scripts.

## Assembly Definitions
The Scripts folder, which contains all source code files, has its own assembly definition, so you're project will probably need its own assembly definition which reference this FuseTools assembly if you want to reference the scripts in your code.

This repo's test scripts are covered by separate assembly definitions which probably don't need to be referenced by your project.

## Tests
The repository contains some tests which should appear in your Unity Editor's Test Runner once the contents of this repository are added to your Unity project. Currently only a couple of components have tested.

## The content
The content of this repository is too much of mixed bag collection of utilities to cover in this README, but here are a couple of general notes.

#### UnityEvents and Public Action Methods
In general the components in this repository are designed as much as possible to be used in the Unity editor without any additional coding. Therefore they often invoke UnityEvents and contain "public action methods" (public methods which don't return anything and require zero or one arguments).

Public action methods can be assigned to UnityEvents which allows different behaviours to be linked together. This is a placeholder for the lack of visual scripting support in Unity (like Unreal's blueprints, TouchDesigner or Max MSP). Note that without the corresponding Editor UI and lack of support by the Unity Editor for methods with multiple arguments and fetching data, this quickly becomes a convoluted mess and should probably mostly be used for prototyping.

#### Ext components
Inside the Scripts/Ext folder there's a collection of components that are each designed to extend the behaviour of another, existing class (like the DirectorExt which serves as a companion to a PlayableDirector component or the TransformExt which provides additional functionality for a Transform instance).

These "extension" components generally provide public action methods that can be used to manipulate Transform instances in reaction to UnityEvents that would otherwise not be possible without writing custom code. Additionally, these components might also Invoke their own UnityEvents that can be used to assign your own logic to be executed in reaction to the extended components' activities/states/changes.
