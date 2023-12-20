// @codekit-prepend "jquery.smartmenus.js";
// @codekit-prepend "pushbar.js";

new Pushbar({
	blur: true,
    overlay: true,
});

// SmartMenus init
$(function() {
 	
  	$('#main-menu').smartmenus({
	    markCurrentItem: true
  	});
  	
	$('#mobile-main-menu').smartmenus({
	    markCurrentItem: true
  	});

});


var $sub_menu = $('#sub-menu');

/* 
 * if parent is a #, toggle children
 * if parent is a #, toggling the title closes children
 * if parent is a link, go to url on first click
*/
$sub_menu.smartmenus( {
    markCurrentItem:     true,
    collapsibleBehavior: 'accordion-link',
    hideOnClick:         false
    
} ).on( 'select.smapi', function( e, item ) {
    
    var obj   = $(this).data( 'smartmenus' ),
        $item = $(item);
        
    if ( obj.isCollapsible() && $item.is('[href="#"]') ) {
        var $sub = $item.dataSM( 'sub' );
        if ( $sub && !$sub.is( ':visible') ) {
            obj.itemActivate( $item, true );
        }
        return false;
    }
} ).bind('click.smapi', function( e, item ) {

    var obj = $( this ).data( 'smartmenus' );
    
    if ( obj.isCollapsible() ) {
        var $sub = $( item ).dataSM( 'sub' );
        if ( $sub && $sub.is( ':visible' ) ) {
            obj.menuHide( $sub );
            return false;
        }
    }
} );
		
//__ keep open on current
$sub_menu.smartmenus( 'itemActivate', $sub_menu.find( 'a.current' ).eq( -1 ) );


//expandables  
$('.faq-slide h3').click(function() {
    
    // on click find next inner div and toggle open/closed
    $(this).next('.faq-slide_inner').slideToggle(500);
      // add this class when open
      $(this).parent().toggleClass('faq-toggle');
}); 

// toggle expandable
$('.expandable-item h3').click(function() {
      // on click find next inner div and toggle open/closed
      $(this).next('.expandable').slideToggle(500);
      // add this class when open
      $(this).parent().toggleClass('ex-toggle');
}); 


// open links with class .external-link in new window  
 var anchors = document.getElementsByTagName('a');

	// Loop through the anchors and add the click handler if it includes the CSS class 'external-link'
	for ( var i in anchors ) 
		if ( anchors[i].className && anchors[i].className.indexOf('external-link') != -1 )
			anchors[i].onclick = function () { return !window.open(this); };
			

//animate in viewport
(function() {
  var elements;
  var windowHeight;

  function init() {
    elements = document.querySelectorAll('.hidden');
    windowHeight = window.innerHeight;
  }

  function checkPosition() {
    for (var i = 0; i < elements.length; i++) {
      var element = elements[i];
      var positionFromTop = elements[i].getBoundingClientRect().top;

      if (positionFromTop - windowHeight <= 0) {
        element.classList.add('animate__fadeInUpBig');
        element.classList.remove('hidden');
      }
    }
  }

  window.addEventListener('scroll', checkPosition);
  window.addEventListener('resize', init);

  init();
  checkPosition();
})();			

