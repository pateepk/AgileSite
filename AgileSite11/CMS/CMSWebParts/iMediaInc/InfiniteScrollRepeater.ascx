<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSWebParts_iMediaInc_InfiniteScrollRepeater" CodeBehind="~/CMSWebParts/iMediaInc/InfiniteScrollRepeater.ascx.cs" %>

<cms:CMSEditModeButtonAdd ID="btnAdd" runat="server" Visible="False" />

<div id="scrollingcontent">
    <div class="scrollingcontentblock" data-page="1">
        <cms:CMSRepeater ID="repItems" runat="server" />
    </div>
</div>

<script>

    /////////////////////////////////////////////////////
    /////////////INFINITE SCROLL ///////////////////////
    ///////////////////////////////////////////////////
    //Thank you John Mueller
	var loader_method = ''; //method to call while waiting for new pages to load	
    var last_scroll = 0;

    //Infinite Scroll Settings
    var use_url_update = "True";    //Update the url as the user scrolls
    var total_pages = 0;  //provided from the back end PagerControl.PageCount
    var currentpg = 1;
    var url = '';  //url of the page including any params
    var param_name = ""; //provided from the web part Query String Key    
    var header_height = 10; //Height of the page header
    var next_data_page = 1; // replaced when loading more
    var prev_data_page = 1; // replaced when loading more
    var next_data_url = replaceUrlParam(url, param_name, next_data_page); // replaced when loading more
    var prev_data_url = replaceUrlParam(url, param_name, prev_data_page); // replaced when loading more   
    var next_data_cache;
    var prev_data_cache;
    var is_loading = 0; // simple lock to prevent loading when loading   
    function loadFollowing() {
        if (next_data_page > total_pages) {
        } else {
            is_loading = 1; // note: this will break when the server doesn't respond
            function showFollowing(data) {
                $('div.scrollingcontentblock:last').after(data);
                if (next_data_page <= total_pages) {
                    next_data_page++;
                }
                next_data_url = replaceUrlParam(url, param_name, next_data_page);
                next_data_cache = false;
                $.get(next_data_url, function (preview_data) {
                    next_data_cache = parseHTML(preview_data, next_data_page);
                    //Call loading                   
                }).done(function () {
                    //End loading                    
                });
            }
            if (next_data_cache) {
                showFollowing(next_data_cache);
                is_loading = 0;
            } else {
				if (loader_method != '') {
					    try{
                            window[loader_method]();
					    } catch(err){}
                    }
                $.get(next_data_url, function (data) {
                    showFollowing(parseHTML(data,next_data_page));
                    is_loading = 0;
                    //Call loading
                }).done(function () {
                    //End loading
					if (loader_method != '') {
                        try{
                            window[loader_method]();
					    } catch(err){}
                    }
                });
            }
        }
    }

    function loadPrevious() {
        if (prev_data_page == 0) {
        } else {
            is_loading = 1; // note: this will break when the server doesn't respond
            function showPrevious(data) {
                $('div.scrollingcontentblock:first').before(data);
                item_height = $("div.scrollingcontentblock:first").height();
                window.scrollTo(0, $(window).scrollTop() + item_height + header_height); // adjust scroll                            
                prev_data_page--;
                prev_data_url = replaceUrlParam(url, param_name, prev_data_page);
                prev_data_cache = false;
                $.get(prev_data_url, function (preview_data) {
                    prev_data_cache = parseHTML(preview_data, prev_data_page);
                    //Call loading
                }).done(function () {
                 //End loading
                });
            }
            if (prev_data_cache) {
                showPrevious(prev_data_cache);
                is_loading = 0;
            } else {
				if (loader_method != '') {
                        try{
                            window[loader_method]();
					    } catch(err){}
                    }
                $.get(prev_data_url, function (data) {
                    showPrevious(parseHTML(data, prev_data_page));
                    is_loading = 0;
                    //Call loading                    
                }).done(function () {
                    //End loading 
					if (loader_method != '') {
                        try{
                            window[loader_method]();
					    } catch(err){}
                    }
                });
            }
        }
    };
	
    function initPaginator() {
        $(window).scroll(function () {
            // handle scroll events to update content
            var scroll_pos = $(window).scrollTop();
            if (scroll_pos >= 0.5 * ($(document).height() - $(window).height())) {
                if (is_loading == 0 && (currentpg < total_pages)) {
                    loadFollowing();
                    currentpg++;
                }
            }
            if (scroll_pos <= header_height) {
                if (is_loading == 0 && prev_data_page >= 1) {
                    //ensure the page doesnt already exist
                    var elements = $('#scrollingcontent').find('.scrollingcontentblock[data-page=' + prev_data_page + ']');
                    if (elements.length < 1) {
                        loadPrevious();
                    }
                }
            }
            if (use_url_update == "True") {
				replaceURL(scroll_pos, last_scroll);
			}
        });
        $(document).ready(function () {
            url = window.location.href;
            var pg_param = getParameterByName(param_name);
            //Check to see if there is a page parameter
            if (pg_param != "") {
                currentpg = pg_param;
                $('.scrollingcontentblock').attr("data-page", currentpg);
                if (currentpg > 1) {
                    prev_data_page = parseInt(currentpg) - 1;
                    prev_data_url = replaceUrlParam(url, param_name, prev_data_page);
                    loadPrevious();
                }
            }
            next_data_page = parseInt(currentpg) + 1;
            next_data_url = replaceUrlParam(url, param_name, next_data_page);
            // if we have enough room, and there are more pages to load, load the next batch
            //if ($(window).height() > $("#scrollingcontent").height()) {
                if (next_data_page <= total_pages) {
                    loadFollowing();
                }
            //}
        });
    }

    
    //InitPaginator() is called from the back end if the infinite scroll is enabled

    ///////////////////////////////////////////////////
    /////////////////////LAZY LOAD////////////////////
    /////////////////////////////////////////////////
    //Lazy Load Settings
    var lazy_button_class = ''; //button class for the lazy loader to load more pages
    
//Handle Lazy Load Option
    function InitLazyLoad() {
		$(window).scroll(function(){
			var scroll_pos = $(window).scrollTop();
			if (use_url_update == "True") {
				replaceURL(scroll_pos, last_scroll);
			}
		});
	
        $(document).ready(function () {
            url = window.location.href;
            //if the pages load with a page param we can only load the 
            //following pages, not previous pages
            var pg_param = getParameterByName(param_name);
            if (pg_param != '') {
                currentpg = pg_param;
                $('.scrollingcontentblock').attr("data-page", currentpg);                
            }
			next_data_page = parseInt(currentpg) + 1;
            if (next_data_page <= total_pages) {                
				//Load next page into cache
				next_data_url = replaceUrlParam(url, param_name, next_data_page);
				loading = 1;				
				$.get(next_data_url, function (data) {				                  
					//parse & append the data
					next_data_cache = parseHTML(data, next_data_page);
				}).done(function () {
					//End loading
					if (loader_method != '') {
						try{
                            window[loader_method]();
					    } catch(err){}
					}
					loading = 0;
				});				
			} else {
				$("." + lazy_button_class).hide();
			}	
			
            //lazy button click to load more
            $("." + lazy_button_class).click(function () {
				if(loading == 0){
					//start the loader
					if (loader_method != '') {
						try{
                            window[loader_method]();
					    } catch(err){}
					}
					//append the cached next page
					$('div.scrollingcontentblock:last').after($(next_data_cache).hide().fadeIn());
					currentpg++;
					//then load the next cache
					next_data_page = parseInt(currentpg) + 1;
					if (next_data_page <= total_pages) {                
						//Load next page into cache
						next_data_url = replaceUrlParam(url, param_name, next_data_page);
						loading = 1;					
						$.get(next_data_url, function (data) {				                  
							//parse & append the data
							next_data_cache = parseHTML(data, next_data_page);	
						}).done(function () {
							//End loading
							if (loader_method != '') {
								try{
									window[loader_method]();
								} catch(err){}
							}
							loading = 0;
						});				
					}
					else {
						$("." + lazy_button_class).hide();
					}
				}
            });
        });		
    }

		
	
    ////////////////////////////////////////////////////////
    //Global functions
    //////////////////////////////////////////////////////

    //Since the whole page is returned - we just need the new page data
    function parseHTML(data, pg) {
        var contents = $(data).find("#scrollingcontent").html();
        //remove the pagination
        contents = $($(contents).html()).not(".PagerControl");
        contents = $('<div/>').append($(contents).clone()).html();
        contents = "<div class='scrollingcontentblock'>" + contents + "</div>";
        //update the page num
        contents = $(contents + ".scrollingcontentblock").attr("data-page", pg);
        return contents;
    }

	function replaceURL(scroll_pos, last_scroll){
	// Adjust the URL based on the top item shown
		if (Math.abs(scroll_pos - last_scroll) > $(window).height() * 0.1) {
			last_scroll = scroll_pos;
			$('.scrollingcontentblock').each(function (index) {
				if (mostlyVisible(this)) {
					var page_replace = replaceUrlParam(url, param_name, $(this).attr("data-page"));
					history.replaceState(null, null, page_replace);                        
					//Add GA code here for page clicks
					return (false);
				}
			});
		}
	}
	
	function mostlyVisible(element) {
        // if ca 25% of element is visible
        var scroll_pos = $(window).scrollTop();
        var window_height = $(window).height();
        var el_top = $(element).offset().top;
        var el_height = $(element).height();
        var el_bottom = el_top + el_height;
        return ((el_bottom - el_height * 0.25 > scroll_pos) &&
          (el_top < (scroll_pos + 0.5 * window_height)));
    }
	
    //Handles the url in case there are any parameters
    function replaceUrlParam(url, paramName, paramValue) {
        var pattern = new RegExp('\\b(' + paramName + '=).*?(&|$)')
        if (url.search(pattern) >= 0) {
            return url.replace(pattern, '$1' + paramValue + '$2');
        }
        return url + (url.indexOf('?') > 0 ? '&' : '?') + paramName + '=' + paramValue
    }

    function getParameterByName(name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
        return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    }

</script>
<asp:Label ID="lblJS" runat="server"></asp:Label>

