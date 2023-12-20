import { Component, Prop, Vue } from "vue-property-decorator";

@Component
export class BaseTabs extends Vue {
  tabs = null;

  // Pass a function which will be invoked with the selected tab
  @Prop()
  onTabChange: any;

  created() {
    this.tabs = this.$children;
  }

  selectTab(selectedTab) {
    this.tabs.forEach((tab) => {
      if (tab.identifier === selectedTab.identifier) {
        if (this.onTabChange) {
          this.onTabChange(selectedTab);
        }
        tab.isActive = true;
      } else {
        tab.isActive = false;
      }
    });
  }
}
