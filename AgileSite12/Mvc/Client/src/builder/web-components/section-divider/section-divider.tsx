import { Component, Prop } from "@stencil/core";

@Component({
  tag: "kentico-section-divider",
  styleUrl: "section-divider.less",
  shadow: true,
})
export class SectionDivider {

  @Prop() showComponentList: boolean;
  @Prop() addTooltip: string;

  render() {
    return (
      <div class="ktc-section-divider">
        <kentico-add-component-button
          is-thin="true"
          is-active={this.showComponentList}
          tooltip={this.addTooltip}
        />
      </div>
    );
  }
}
