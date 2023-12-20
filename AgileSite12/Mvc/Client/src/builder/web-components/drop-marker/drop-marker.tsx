import { Component } from "@stencil/core";

@Component({
  tag: "kentico-drop-marker",
  styleUrl: "drop-marker.less",
  shadow: true,
})
export class DropMarker {
  render() {
    return (
      <div class="ktc-drop-marker-wrapper">
        <div class="ktc-drop-marker"></div>
      </div>
    );
  }
}
