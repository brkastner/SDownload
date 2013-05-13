/// <reference path="jquery-1.7.1-vsdoc.js" />

$(function() {
    function addDownloadButton() {

        var added = false;

        //single player
        $(".listenContent .sound.single").not(".playlist").not(".sd-added").each(function() {

            var self = $(this);
            self.addClass("sd-added");

            var buttonGroup = self.find(".sc-button-group").first();

            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-medium").addClass("sc-button-responsive").addClass("sc-button-download");
            downloadButton.html("SDownload");

            var url = window.location.href.split("?")[0];

            downloadButton.attr("data", url);

            downloadButton.appendTo(buttonGroup);
            added = true;
            downloadButton.on("click", function(event) {
                event.stopPropagation();
                downloadClick(event.target);
            });

        });
        
        //list
        $(".sound").not(".sdownload-added").each(function() {

            var self = $(this);
            self.addClass("sdownload-added");

            var buttonGroup = self.find(".sc-button-group").first();

            var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-small").addClass("sc-button-responsive").addClass("sc-button-download");
            downloadButton.html("SDownload");

            var artCoverLink = self.find(".sound__coverArt").first();
            var url = artCoverLink.attr("href");
            url = "https://soundcloud.com" + url;

            downloadButton.attr("data", url);

            downloadButton.appendTo(buttonGroup);

            downloadButton.on("click", function(event) {
                event.stopPropagation();
                downloadClick(event.target);
            });

        });


        function downloadClick(target) {

            var songHref = $(target).attr("data");

            $(target).off("click");

            if (songHref && songHref.length > 0) {
                $(target).html("Sent to Application!");
                window.location = "sdownload://" + songHref;
            }
        }
    }

    setInterval(function() {
        addDownloadButton();
    }, 100);

});