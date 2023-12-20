import { Loader } from "./loader";

describe("loader", () => {
  it("should build", () => {
    expect(new Loader()).toBeTruthy();
  });
});
