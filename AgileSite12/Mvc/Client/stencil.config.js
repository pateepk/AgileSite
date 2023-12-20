const less = require('@stencil/less');

exports.config = {
  namespace: 'components',
  srcDir: 'src',
  outputTargets: [
    { type: 'dist' }
  ],
  hashFileNames: false,
  plugins: [
    less({
      injectGlobalPaths: [
        'src/builder/assets/styles/generated/variables.less',
      ],
    })
  ],
  globalScript: 'src/builder/stencil-global.ts'
};

exports.devServer = {
  root: 'www',
  watchGlob: '**/**'
}
