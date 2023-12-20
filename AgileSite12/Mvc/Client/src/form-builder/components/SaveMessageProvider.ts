import { connect } from "@/builder/helpers/connector";

import SaveMessage from "@/form-builder/components/SaveMessage.vue";
import { FormBuilderState, SaveMessageComponentState } from "@/form-builder/declarations";

const mapStateToProps = (state: FormBuilderState): SaveMessageComponentState => ({
  savingInProgress: state.savingInProgress,
});

export default connect(mapStateToProps)(SaveMessage);
