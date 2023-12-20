import { storiesOf } from "@storybook/vue";
import { withOptions } from "@storybook/addon-options";

import Colors from "./Colors/Colors.vue";
import Icons from "./Icons.vue";

storiesOf("Styles", module)
  .addDecorator(withOptions)
  .addParameters({ options: { showAddonPanel: false } })
  .add("Colors", () => ({
    components: {
      "cms-colors": Colors,
    },
    template: "<cms-colors />",
  }))
  .add("Icons", () => ({
    components: {
      "cms-icons": Icons,
    },
    template: "<cms-icons />",
  }));

storiesOf("Components|Alert", module)
  .addDecorator(withOptions)
  .addParameters({ options: { showAddonPanel: true } })
  .add("with message", () => ({
    template: "<kentico-alert-box message=\"Error message\"></kentico-alert-box>",
  }));
