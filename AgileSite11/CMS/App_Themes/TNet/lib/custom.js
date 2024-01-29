//@codekit-prepend "vendor/jquery.multilevelpushmenu.js"
//@codekit-prepend "vendor/jquery.smartmenus.js"
//@codekit-prepend "vendor/fancy/jquery.fancybox.js"
//@codekit-prepend "vendor/fancy/jquery.fancybox-media.js"
  

$(document).ready(function(){  

/* HTML markup implementation, overlap mode
	$( '#topics-nav' ).multilevelpushmenu({
    	containersToPush: [$( '#pushobj' )],
    	backItemIcon: 'icon icon-caret-left', 
    	groupIcon: 'icon icon-angle-right',
    	mode: 'cover',
    	fullCollapse: true
	});
*/
	
    //menu-toggle open
    var $transformer = $('.transformer'),
    $menuToggle = $('#expand');

    // Attaches event handler when .menu-toggle is clicked
    $('#expand').on('click', function(event) {
        event.preventDefault();
        $transformer.addClass('is-open');
    });
    
    //menu-toggle close
    var $transformer = $('.transformer'),
    $menuToggle = $('#collapse');

    // Attaches event handler when .menu-toggle is clicked
    $('#collapse').on('click', function(event) {
        event.preventDefault();
        $transformer.removeClass('is-open');
    });
    
    // add classes to smart (main) menu to make script work
    //$('.off-canvas ul').addClass('sm sm-mint collapsed');

//smartmenus
    $('.sm-mint').smartmenus();

//smooth scroll to top
    $('.scroll-up').click(function(){
        //scroll function- moves up on click
        $("html, body").animate({ scrollTop: 0 }, 600);  
        return false;
    });




});




						
