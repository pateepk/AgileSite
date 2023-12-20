// Override CustomEvent polyfill before stencil's implementation, which causes errors in IE11.
// This solution is temporary until we upgrade stencil to newer version,
// where the polyfill implementation has already been fixed.
import "@/builder/custom-event";
import "@/builder/mouse-event";

// Load stencil components
import "../../dist/components";

import { initContainer, resolveService } from "@/builder/container";
import { linkHelper } from "@/builder/helpers";
import { registerInlineEditor } from "@/builder/services/inline-editors";

import "./assets/styles/global-styles.less";
import { pageBuilderContainer } from "./container-config";
import { PageBuilder } from "./PageBuilder";
import { PageBuilderOptions } from "./types";

initContainer(pageBuilderContainer);

window.kentico = window.kentico || {};
window.kentico.pageBuilder = {
  init: async (options: PageBuilderOptions) => {
    const pageBuilder = resolveService<PageBuilder>(PageBuilder);
    await pageBuilder.initializePageBuilder(options);
  },

  registerInlineEditor,
};

linkHelper.disableDocumentLinks();
