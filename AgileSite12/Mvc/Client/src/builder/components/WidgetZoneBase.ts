import { Prop, Vue } from "vue-property-decorator";

import { WidgetZoneComponentContext } from "@/builder/declarations";

export abstract class WidgetZoneBase extends Vue implements WidgetZoneComponentContext {
  @Prop({ required: true }) public sectionIdentifier: string;
  @Prop({ required: true }) public zoneIndex: number;
  @Prop({ required: true }) public areaIdentifier: string;
}
