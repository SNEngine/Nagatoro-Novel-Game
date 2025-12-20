# XNode Node Creation Mechanism

## Overview

XNode is a visual node editor system for Unity that allows developers to create node-based graphs. The mechanism for creating nodes involves several key components working together to provide a flexible and extensible node system.

## Core Node Creation Process

### 1. Node Class Structure
- The `Node` class is the base class for all nodes in XNode
- It inherits from `ScriptableObject`, which allows nodes to be serialized and stored as Unity assets
- The class is abstract and must be extended to create actual node types

### 2. Node Creation Methods

#### Runtime Creation
```csharp
// Adding a node to a graph by type (generic version)
T AddNode<T>() where T : Node

// Adding a node to a graph by System.Type
Node AddNode(Type type)
```

The `AddNode` method in `NodeGraph.cs` is responsible for:
- Setting `Node.graphHotfix` to the current graph to ensure proper initialization
- Creating a new instance of the node using `ScriptableObject.CreateInstance(type)`
- Setting the graph reference on the node
- Adding the node to the graph's node list

#### Editor Creation
- Nodes can be created through the context menu in the Node Editor Window
- The `CreateNode` method in `NodeGraphEditor.cs` handles the creation process:
  - Uses Undo system for proper Unity integration
  - Sets the node position based on mouse click location
  - Sets default node name
  - Adds the node to the asset if it's an asset-based graph
  - Saves automatically if auto-save is enabled

### 3. Node Port System

#### Static Ports (Field-based)
- Created using `[Input]` and `[Output]` attributes on class fields
- Defined at design time as class members
- Automatically discovered and created through reflection

#### Dynamic Ports
- Created at runtime using methods like `AddDynamicInput()` and `AddDynamicOutput()`
- Useful for ports that need to be created based on runtime conditions

### 4. Port Attributes

#### Input Attribute
```csharp
[Input]
public float inputValue;
```
Options:
- `backingValue`: Controls when to show the backing field value
- `connectionType`: Whether to allow multiple connections or override
- `typeConstraint`: Type constraints for connections
- `dynamicPortList`: For dynamic port lists

#### Output Attribute
```csharp
[Output]
public float outputValue;
```
Options:
- `backingValue`: Controls when to show the backing field value
- `connectionType`: Whether to allow multiple connections or override
- `typeConstraint`: Type constraints for connections
- `dynamicPortList`: For dynamic port lists

### 5. Node Registration and Discovery

#### Node Creation Menu
- The `[CreateNodeMenu]` attribute allows customizing where a node appears in the context menu
- Nodes without this attribute still appear based on their namespace/type name

#### Node Type Cache
- `NodeDataCache.cs` maintains a cache of node types and their ports
- Uses reflection to discover all node types that inherit from `XNode.Node`
- Builds port information at startup to avoid runtime reflection overhead

### 6. Node Lifecycle

#### Initialization Process
1. Node is created via `ScriptableObject.CreateInstance()`
2. `OnEnable()` is called where:
   - Graph reference is set using `graphHotfix` if needed
   - `UpdatePorts()` is called to ensure all ports are created
   - `Init()` is called for custom initialization

#### Port Management
- `UpdatePorts()` synchronizes static and dynamic ports
- Ensures that the node's ports match the class definition
- Handles reconnection of existing connections when port definitions change

### 7. Node Customization Attributes

#### NodeTint
- `[NodeTint("#3b3b3b")]` - Sets the color of the node in the editor

#### NodeWidth
- `[NodeWidth(230)]` - Sets the default width of the node

#### DisallowMultipleNodes
- `[DisallowMultipleNodes(1)]` - Prevents adding more than a specified number of nodes of the same type to a graph

#### RequireNode
- `[RequireNode(typeof(StartNode))]` - Ensures a specific node type exists in the graph and prevents deletion

## Extension Mechanism

### BaseNode (XNodeExtensions)
The project includes an extended `BaseNode` class that provides:
- GUID management for unique identification
- Execute method for running node logic
- Helper methods for getting data from ports
- Support for async operations

### Node Graph Execution
- `BaseGraph` provides execution functionality
- Includes queue system for node execution
- Supports topological sorting of nodes
- Provides pause, continue, and stop functionality

## Key Features

1. **Type Safety**: Strong typing with connection validation
2. **Flexible Connections**: Multiple connection types and type constraints
3. **Runtime Creation**: Nodes can be created and modified at runtime
4. **Editor Integration**: Full Unity editor support with context menus
5. **Serialization**: Proper Unity serialization and asset management
6. **Performance**: Cached port information to avoid runtime reflection
7. **Extensibility**: Easy to create custom node types and editors

This mechanism provides a robust foundation for creating visual scripting systems, dialogue systems, behavior trees, and other node-based tools in Unity.