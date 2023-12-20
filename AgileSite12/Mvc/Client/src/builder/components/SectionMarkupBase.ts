import { Component as VueComponent } from "vue";
import { Component, Prop, Vue } from "vue-property-decorator";

import { linkHelper } from "@/builder/helpers";
import { replacePlaceholdersWithScripts, stripScriptElements } from "@/builder/helpers/markup-helper";

@Component
export class SectionMarkupBase extends Vue {
  widgetZone: VueComponent;

  @Prop({ required: true }) sectionIdentifier: string;
  @Prop({ required: true }) sectionMarkup: string;
  @Prop({ required: true }) areaIdentifier: string;

  render(h) {
    const strippedMarkupResult = stripScriptElements(this.sectionMarkup);

    return h({
      template: `<div ref="sectionMarkup">${strippedMarkupResult.strippedMarkup}</div>`,
      components: {
        WidgetZone: this.widgetZone,
      },
      data: () => {
        return {
          sectionIdentifier: this.sectionIdentifier,
          areaIdentifier: this.areaIdentifier,
        };
      },
      mounted() {
        replacePlaceholdersWithScripts(this.$refs.sectionMarkup, strippedMarkupResult.scriptMappings);
        linkHelper.disableElementLinks(this.$refs.sectionMarkup);
      },
      updated() {
        replacePlaceholdersWithScripts(this.$refs.sectionMarkup, strippedMarkupResult.scriptMappings);
        linkHelper.disableElementLinks(this.$refs.sectionMarkup);
      },
    });
  }
}
