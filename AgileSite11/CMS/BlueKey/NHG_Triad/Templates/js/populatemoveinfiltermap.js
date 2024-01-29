// JavaScript document

//; (function (j) {
var j = jQuery.noConflict();

//var geocoder = new google.maps.Geocoder();

var latlng = new google.maps.LatLng(36.075742, -79.785461);

var myOptions = {
    zoom: 10,
    center: latlng,
    mapTypeId: google.maps.MapTypeId.ROADMAP
};

var map = new google.maps.Map(document.getElementById("google_map"), myOptions);

    function getLocationPosition(address, lat, lon) {
        var position = null;

        
        
        if (lat != '' && lon != '') {
            position = new google.maps.LatLng(lat, lon);
        }
        else {
            geocoder.geocode({ 'address': address }, function (results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    position = results[0].geometry.location;
                }
            });

        }
        return position;
    }

    function addNewNeighborhood(name, position, details, icon) {
        if (position != null) {
            var infoWindow = new google.maps.InfoWindow({
                content: details
            });

            var marker = new google.maps.Marker({
                map: map,
                position: position,
                title: name,
                icon: icon
            });

            google.maps.event.addListener(marker, 'click', function () {
                infoWindow.open(map, marker);
            });

            //map.setCenter(position);
        }
    }

    function populateMoveinFilterMap(neighborhoods) {
        var distinctIds = [];
        var infoArray = [];

        var copy = neighborhoods
        j.each(neighborhoods, function (item, val) {
            if (j.inArray(neighborhoods, item.neighborhoodId) === -1) {
                distinctIds.push({ "neighborhoodId": val.neighborhoodId, "neighborhood": val.neighborhood });
            }
        });

        for (var i = 0; i < distinctIds.length; i++)
        {
            var tempArray = [];
            j.each(copy, function (item, val) {
                if (val.neighborhoodId === distinctIds[i].neighborhoodId) {
                    tempArray.push(val.listing);
                }
            });

            copy = j.grep(copy, function (e, val) {
                return e.neighborhoodId != distinctIds[i].neighborhoodId;
            });

            if (tempArray.length > 0)
            {
                infoArray.push({ "neighborhoodId": distinctIds[i].neighborhoodId, "neighborhood": distinctIds[i].neighborhood, "listings": tempArray });
            }
        }

        j.each(infoArray, function (item, val) {
            var listingHtml = "<ul style='list-style-type: none;'>";
            for (var i = 0; i < val.listings.length; i++)
            {
                listingHtml += "<li>Lot " + val.listings[i].listingLot + "</li>";
            }
            listingHtml += "</ul>";
            var position = getLocationPosition(val.neighborhood.neighborhoodAddress, val.neighborhood.neighborhoodLatitude, val.neighborhood.neighborhoodLongitude);
            addNewNeighborhood(val.neighborhood.neighborhoodName, position, "<div style='height: 100px'><a href=" + val.neighborhood.neighborhoodUrl + ">" + val.neighborhood.neighborhoodName + "</a>" + listingHtml + "<a href='http://maps.google.com/maps?daddr=" + position + "' target='_blank'>Get Directions</a></div>", val.neighborhood.neighborhoodIcon + ".png");
        })
    }
//})(jQuery);