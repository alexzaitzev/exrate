var BankViewModel = function () {
    var self = this;
    self.banks = ko.observableArray();
    self.bank = ko.observable();
    self.department = "филиал";
    self.markerImage = "../../images/exchange.png";
    self.markersArray = [];
    self.opnInfoWindow = "";

    self.getBanks = function () {
        $("#btable tbody").after("<tr><td colspan='5'><img src='/images/loading.gif'></td></tr>");
        $.post("/bank/getbanks", { CityId: erm.city().Value }, function (resp) {
            $("#btable tr:last").remove();
            self.banks(resp);
            $('#btable tbody tr').click(self.showBank);
        });
    };
    self.showBank = function () {
        var id = $(this).attr('id');
        router.setRoute('/bank/' + id);
    };
    self.getBank = function (code) {
        $.post("/bank/getbank", { CityId: erm.city().Value, Bank: code }, function (resp) {
            //if bank doesn't exist in current city
            if (!resp.Name) {
                //redirect to the banks page
                router.setRoute('/' + erm.tabs[1]);
                return;;
            }
            self.bank(resp);
            $('#' + erm.tab).show();
            erm.mapInitialize();
            self.createMarkers();
        });
    };
    self.createMarkers = function () {
        var coords = self.bank().Addresses;
        self.deleteMarkers();
        for (var i = 0; i < coords.length; i++) {
            var myLatLng = new google.maps.LatLng(coords[i].Latitude, coords[i].Longitude);
            var marker = new google.maps.Marker({
                position: myLatLng,
                map: erm.map,
                icon: self.markerImage,
                zIndex: i
            });
            var infowindow = new google.maps.InfoWindow({
                content: "<div class='infobox'>" + coords[i].Address + "</div>"
            });
            google.maps.event.addListener(marker, 'click', (function (m, iw) {
                return function () {
                    if (!$.isEmptyObject(self.opnInfoWindow))
                        self.opnInfoWindow.close();
                    iw.open(erm.map, m);
                    self.opnInfoWindow = iw;
                };
            })(marker, infowindow));
            self.markersArray.push(marker);
        }
    };
    self.deleteMarkers = function () {
        if (self.markersArray) {
            for (i in self.markersArray)
                self.markersArray[i].setMap(null);
            self.markersArray.length = 0;
            google.maps.event.clearListeners(erm.map, 'bounds_changed');
        }
    };
    self.CorrectEnding = function (count) {
        if (count > 20)
            count = count % 10;
        if (count == 1)
            return self.department;
        if (count < 5 && count != 0)
            return self.department + "а";
        return self.department + "ов";
    };
};