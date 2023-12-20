import { State } from "@/builder/declarations";
import { connect, MapDispatchToProps, MapStateToProps } from "@/builder/helpers/connector";
import { hideDropMarker, showDropMarker } from "@/builder/store/drag-and-drop/actions";

import { DropZoneComponentActions, DropZoneComponentState } from "./drop-zone-types";
import DropZone from "./DropZone.vue";

type DropZoneStateToProps = MapStateToProps<State, object, DropZoneComponentState>;
type DropZoneDispatchToProps = MapDispatchToProps<DropZoneComponentActions>;

const mapStateToProps: DropZoneStateToProps = ({ dragAndDrop }) => ({
  dragAndDrop,
});

const mapDispatchToProps: DropZoneDispatchToProps = () => ({
  hideDropMarker,
  showDropMarker,
});

export default connect(mapStateToProps, mapDispatchToProps)(DropZone);
