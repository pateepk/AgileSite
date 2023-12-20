import { Component, Event, EventEmitter, Prop, State, Watch } from "@stencil/core";

import { GetString } from "../../selector-types";
import { TreeNode } from "./tree-node";

@Component({
  tag: "kentico-media-files-tree",
  styleUrl: "media-files-tree.less",
  shadow: true,
})
export class MediaFilesTree {
  @Prop() getString: GetString;
  @Prop() selectedPath: string;
  @Prop() treeStructure: TreeNode;

  @State() openedNodePaths: string[] = [""];

  @Event() selectNode: EventEmitter<string>;

  @Watch("treeStructure")
  watchHandler() {
    this.openedNodePaths = [this.treeStructure.path];
    this.openNodePath(this.selectedPath);
  }

  openNodePath = (nodePath: string) => {
    this.openedNodePaths.push(nodePath);
    let lastSlash = nodePath.lastIndexOf("/");
    while (lastSlash > -1) {
      const parentPath = nodePath.substr(0, lastSlash);
      this.openedNodePaths.push(parentPath);
      lastSlash = parentPath.lastIndexOf("/");
    }
  }

  closeItem = (node: TreeNode) => {
    const index = this.openedNodePaths.indexOf(node.path);
    if (index !== -1) {
      this.openedNodePaths.splice(index, 1);
      this.openedNodePaths = [...this.openedNodePaths];
    }
  }

  openItem = (node: TreeNode) => {
    this.openedNodePaths = [...this.openedNodePaths, node.path];
  }

  selectItem = (node: TreeNode) => {
    if (node.path === this.selectedPath) {
      return;
    }
    this.selectNode.emit(node.path);
  }

  isOpened = (node: TreeNode): boolean => {
    return this.openedNodePaths.indexOf(node.path) > -1;
  }

  hasChildren = (node: TreeNode): boolean => {
    return node.children && node.children.length > 0;
  }

  render() {
    const renderNodeName = (node: TreeNode) =>
      <a class={node.path === this.selectedPath ? "ktc-treeview-active" : ""}
        onClick={() => this.selectItem(node)}>
        {node.name}
      </a>;

    const renderInnerNode = (node: TreeNode) =>
      <li>
        {
          this.isOpened(node) ?
            <i class="icon-minus-circle" onClick={() => this.closeItem(node)} title={this.getString("kentico.components.mediafileselector.collapse")}></i>
            : <i class="icon-plus-circle" onClick={() => this.openItem(node)} title={this.getString("kentico.components.mediafileselector.expand")}></i>
        }
        {renderNodeName(node)}
        {this.isOpened(node) ? <ul> {node.children.map(renderNode)} </ul> : null}
      </li>;

    const renderLeafNode = (node: TreeNode) => <li>{renderNodeName(node)}</li>;

    const renderNode = (node: TreeNode) =>
      this.hasChildren(node) ? renderInnerNode(node) : renderLeafNode(node);

    return (
      <div class="ktc-treeview">
        <ul>
          {renderNode(this.treeStructure)}
        </ul>
      </div>
    );
  }
}
