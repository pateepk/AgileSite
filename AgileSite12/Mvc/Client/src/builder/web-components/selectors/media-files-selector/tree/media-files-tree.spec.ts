import { MediaFilesTree } from "./media-files-tree";

describe("media-files-tree", () => {
  it("should build", () => {
    expect(new MediaFilesTree()).toBeTruthy();
  });
});
