// JavaScript Document

;(function($) {

$(document).ready(function() {
	
	var overallHeight = $(window).height();
	var overallWidth = $(window).width();

	$(window).resize(function() {
		overallHeight = $(window).height();
		overallWidth = $(window).width();

		moveSearch();

		iSubscribeHeight = $('.iRowLeft .subscribeAd').outerHeight();
		iRowRightHeight = $('.iRowRight img').outerHeight();
		iRowHeightFix();
	});

	//MENU TOGGLE
	$('.menuToggle').click(function() {
		if ($(this).hasClass('on')) {
			$('.mainNav').removeClass('on');
		} else {
			$('.mainNav').addClass('on');
		}
	});

	// MAIN NAV SETUP
	$('.mainNav ul li').each(function() {
		if ($('ul', this).length) {
			$(this).addClass('hasChildren');
			$(this).append('<div class="dropToggle"></div>');
		}
	});

	$('.dropToggle').click(function() {
		if ($(this).hasClass('on')) {
			$(this).removeClass('on');
			$(this).parent().find('> ul').slideUp(300, 'easeInOutQuart');
		} else {
			$(this).addClass('on');
			$(this).parent().find('> ul').slideDown(500, 'easeOutQuart');
		}
	});

	$('.mainNav .hasChildren').hover(function() {
		$('> ul', this).stop(true, true).slideDown(200, 'easeOutQuart');
	}, function() {
		$('> ul', this).stop(true, true).slideUp(300, 'easeOutQuart');
	});

	$('.mainNavClose').click(function() {
		if ($('.mainNav').hasClass('on')) {
			$('.mainNav').removeClass('on');
		} else {
			$('.mainNav').addClass('on');
		}
	});

	// MAIN SEARCH SLIDER
	$(function () {
	    var low = getUrlVars()["low"];
	    var high = getUrlVars()["high"];

		$( ".slider-range" ).slider({
			range: true,
	      	min: 100,
			max: 750,
			values: [ low == null ? 0 : low, high == null ? 750 : high ],
	      	slide: function( event, ui ) {
	        	
	        	$( ".lowValue span" ).text( ui.values[ 0 ]);
	        	var currMin = $(".slider-range").slider('option', 'min');
		    	if (ui.values[0] <= currMin) {
		        	$( ".lowValue span" ).addClass('atMin');
		    	} else {
		    		$( ".lowValue span" ).removeClass('atMin');
		    	}

		    	$( ".highValue span" ).text( ui.values[ 1 ]);
	        	var currMax = $(".slider-range").slider('option', 'max');
		    	if (ui.values[1] >= currMax) {
		        	$( ".highValue span" ).addClass('atMax');
		    	} else {
		    		$( ".highValue span" ).removeClass('atMax');
		    	}
                		    	
		    	if ($(this).hasClass("neighborhoodFilterSliderRange") ||
                    $(this).hasClass("builderFilterSliderRange") ||
                    $(this).hasClass("moveinFilterSliderRange") ||
                    $(this).hasClass("listingFilterSLiderRange"))
		    	{
		    	    if (ui.values[0] == 100)
		    	    {
		    	        $("#hfLowValue").val(0);
		    	    }
		    	    else
		    	    {
		    	        $("#hfLowValue").val(ui.values[0]);
		    	    }


		    	    if (ui.values[1] == 750)
		    	    {
		    	        $("#hfHighValue").val(100000);
		    	    }
		    	    else
		    	    {
		    	        $("#hfHighValue").val(ui.values[1]);
		    	    }
		    	}
	      	}            
		});
		$(".lowValue span").text($(".slider-range").slider("values", 0));
	    $(".highValue span").text($(".slider-range").slider("values", 1));	    
	});

	function getUrlVars()
	{
	    var vars = [], hash;
	    var url = window.location.href;
	    if (url.indexOf('#') > 0)
	    {
	        url = window.location.href.substring(0, url.indexOf('#'));
	    }

	    console.log(url);
	    var hashes = url.slice(url.indexOf('?') + 1).split('&');
	    for (var i = 0; i < hashes.length; i++)
	    {
	        hash = hashes[i].split('=');
	        vars.push(hash[0]);
	        vars[hash[0]] = hash[1];
	    }

	    return vars;
	}

	// MOBILE ACCORDION	
	$('.mobileAccordionToggle').click(function() {
		if (overallWidth < 992) {
			if ($(this).hasClass('on')) {
				$(this).removeClass('on');
				$(this).nextUntil('.mobileAccordionToggle', '.accordionContent').removeClass('on');
			} else {
				$(this).addClass('on');
				$(this).nextUntil('.mobileAccordionToggle', '.accordionContent').addClass('on');
			}
		}
	});

	$('.neighborhoodSearchFilters .filterWrapper').addClass('on');
	$('.availableHomesMainXXX .filterWrapper').addClass('on');

	// MOVE MAIN SEARCH
	function moveSearch() {
		if (overallWidth >= 992) {
			$('.mainSearchTool').appendTo('.banner');
		} else {
			$('.mainSearchTool').insertAfter('.banner');
		}
	}
	moveSearch();

	// INDEX ROW1 HEIGHT FIX
	var iSubscribeHeight = $('.iRowLeft .subscribeAd').outerHeight();
	var iRowRightHeight = $('.iRowRight img').outerHeight();

	function iRowHeightFix() {
		if (overallWidth >= 992) {
			$('.iRowLeft .subscribeAd').height(iRowRightHeight);
		}
	}
	iRowHeightFix();

	// MAP SIZE CONTROL
	$('a.mapSizeToggle').click(function(e) {
		if ($('.mapWrapper').hasClass('expanded')) {
			$('.mapWrapper').removeClass('expanded');
			$(this).html('View Larger Map');
		} else {
			$('.mapWrapper').addClass('expanded');
			$(this).text('View Smaller Map');
		}
		e.preventDefault();
	});

	// CLOSE MAP
	$('a.mapClose').click(function(e) {
		if ($('.mapWrapper').hasClass('hidden')) {
			$('.mapWrapper').removeClass('hidden');
			$('.mapSizeToggle').show();
			$(this).html('Close Map');
		} else {
			$('.mapWrapper').addClass('hidden');
			$('.mapSizeToggle').hide();
			$(this).text('Open Map');
		}
		e.preventDefault();
	});

	// MAP KEY
	$('.mapKeyToggle').hover(function() {
		$('.mapKey').show();
	}, function() {
		$('.mapKey').hide();
	});

	$('.mapKeyToggle').click(function(e) {
		e.preventDefault();
	});

	// MOBILE FILTER TOGGLE
	$('.builderListingFilter .filterWrapper').addClass('off').removeClass('on');
	$('.builderListingFilter .mobileFilterToggle').text('Show Filters');
	$('.listingFilter .filterWrapper').addClass('off').removeClass('on');
	$('.listingFilter .mobileFilterToggle').text('Show Filters');
	$('.moveInFilters .filterWrapper').addClass('on').removeClass('off');
	$('.moveInFilters .mobileFilterToggle').text('Hide Filters');

	if (window.location.search.indexOf('map=1') > -1) {

	} else {
		$('.neighborhoodSearchFilters .filterWrapper').removeClass('on').addClass('off');
		$('.neighborhoodSearchFilters .mobileFilterToggle').text('Show Filters');
	}

	
	if ($('.mobileFilterToggle').length) {
		var filterText = $(this).text();
	}

	$('.mobileFilterToggle').click(function() {
		if ($('.filterWrapper').hasClass('on')) {
			$('.filterWrapper').removeClass('on');
			$('.filterWrapper').addClass('off');
			$(this).text('Show Filters');
		} else {
			$('.filterWrapper').addClass('on');
			$('.filterWrapper').removeClass('off');
			$(this).text('Hide Filters');
		}
	});

	// AVAILABLE HOMES LISTING MORE ACTION
	$('.availableHomeActions a.showMore').click(function(e) {
		if ($(this).hasClass('on')) {
			$(this).closest('.availableHomeActions').next().slideUp(200);
			$(this).removeClass('on');
			$(this).text('More +');
		} else {
			$(this).closest('.availableHomeActions').next().slideDown(500, 'easeOutQuart');
			$(this).addClass('on');
			$(this).text('Less -');
		}
		e.preventDefault();
	});

	// MOVE IN READY HOMES CARD FLIP
	$('.moveInHome .flipCard').click(function(e) {
		$(this).closest('.moveInHome').addClass('flipped');
		e.preventDefault();
	});

	$('.moveInHomeMoreClose').click(function() {
		$(this).closest('.moveInHome').removeClass('flipped');
	});

	// NEIGHBORHOOD SORTING
	var sortUp = true;
	window.sortUsingNestedText = function (parent, childSelector, keySelector) {
	    var items = parent.children(childSelector).sort(function (a, b) {
	        if (keySelector == ".sortPriceNumber")
            {	            
	            var vA = parseInt($(keySelector, a).text().replace(/\$/g, "").replace(/,/g, ""));
	            var vB = parseInt($(keySelector, b).text().replace(/\$/g, "").replace(/,/g, ""));
	        }
            else
	        {
	            var vA = $(keySelector, a).text();
	            var vB = $(keySelector, b).text();
	        }

	        if (sortUp) {
	        	return (vA < vB) ? -1 : (vA > vB) ? 1 : 0;
	        } else {
	        	return (vA > vB) ? -1 : (vA < vB) ? 1 : 0;
	        }
	    });	    parent.append(items);
	}

	/* setup sort attributes */
	$('.sorterName').data("sortKey", ".sortName");
	$('.sorterPrice').data("sortKey", ".sortPriceNumber");
	$('.sorterCity').data("sortKey", ".sortCity");
	$('.sorterBuilder').data("sortKey", ".sortBuilder");
	$('.sorterCustom').data("sortKey", ".sortCustom");
	$('.sorterSqFt').data("sortKey", ".sortSqFt");
	$('.sorterLot').data("sortKey", ".sortLot");
	$('.sorterBedBath').data("sortKey", ".sortBedBath");
	$('.sorterPlan').data("sortKey", ".sortPlan");
	$('.sorterStories').data("sortKey", ".sortStories");
	$('.sorterReady').data("sortKey", ".sortReady");


    /* sort on button click */
	var lon = getUrlVars()["lon"];
	var lat = getUrlVars()["lat"];
	if (lon == null && lat == null) {
	    $('.sorterName').addClass('up');
	    sortUsingNestedText($('.sortList'), "div", '.sortName');
	}

	$(".sortWrapper .sorter").click(function() {
		if ($(this).hasClass('up')) {
			$('.sorter').removeClass('up');
			$('.sorter').removeClass('down');
			sortUp = false;
			$(this).addClass('down');
		} else if ($(this).hasClass('down')) {
			$('.sorter').removeClass('up');
			$('.sorter').removeClass('down');
			sortUp = true;
			$(this).addClass('up');
		} else {            
			$('.sorter').removeClass('up');
			$('.sorter').removeClass('down');
			if ($(this).hasClass('sorterCustom')) {
			    sortUp = false;
			    $(this).addClass('down');
			}
			else {
			    sortUp = true;
			    $(this).addClass('up');
			}
		}
		sortUsingNestedText($('.sortList'), "div", $(this).data("sortKey"));
		//__doPostBack("jsSort", $(this).attr('class'));
	});


    // SEARCH
	$(".headerSearch > input[type=text]").keypress(function (e) {
	    if (e.which == 13) {
	        $(".headerSearch > input[type=text]").click();
	        return false;
	    }
	});

	$(".headerSearch > input[type=submit]").click(function () {
	    var searchArg = $.trim($(".headerSearch > input[type=text]").val());

	    if ($.isNumeric(searchArg)) {
	        searchArg += "*";
	    }

	    url = "/search-results?searchtext=" + searchArg;

	    location.href = url;
	    return false;
	});

	// MOVE LOGIN
	function moveLogin() {
		if (overallWidth >= 992) {
			$('.topLogin').insertBefore('.logo');
		} else {
			$('.topLogin').prependTo('.mainNav');
		}
	}
	moveLogin();

	$(window).resize(function() {
		moveLogin();
	});
	
	// HIDE STUFF JUST FOR BUILDER DETAIL MOBILE
	if ($('.builderListingFilter').length && overallWidth < 992) {
		$('.showingHomesTotal').insertBefore('.availableHomesSort');
		$('.searchResultsFine').insertBefore('.showingHomesTotal');
	}

	// MOVE FLOOR PLAN BUTTON ON MOBILE
	function moveFloorPlanBtn() {
		if (overallWidth < 992) {
			$('.btnViewFloorPlans').prependTo($('.btnViewFloorPlans').next('.accordionContent'));
		} else {
			$('.btnViewFloorPlans').insertBefore($('.btnViewFloorPlans').parent('.accordionContent'));
		}
	}
	moveFloorPlanBtn();

	$(window).resize(function() {
		moveFloorPlanBtn();
	});

});

$(window).load(function() {
	

});

})(jQuery);