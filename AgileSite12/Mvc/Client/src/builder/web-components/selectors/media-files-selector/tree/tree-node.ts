export interface TreeNode {
  readonly name: string;
  readonly children: TreeNode[];
  readonly path: string;
}
