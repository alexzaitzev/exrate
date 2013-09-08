var StatsViewModel = function () {
    var self = this;
    self.rates = ko.observableArray();
    self.sortTypes = ["stat-name", "stat-buy", "stat-sell"];
    self.sortType = self.sortTypes[1];
    self.sortDirection = -1;
    self.currencies = ["USD", "EUR"];
    self.currency = self.currencies[0];
    self.selBank = ko.observable("");
    self.isInited = false;

    self.sort = function (obj, event) {
        //hack for IE
        if (!event) event = window.event;
        //gets link's id to determine which column to sort
        var choseType = $(event.currentTarget)[0].id;
        //if link was clicked twice - change direction of sorting
        if (self.sortType == choseType)
            self.sortDirection *= -1;
        else self.sortDirection = -1;
        self.sortType = choseType;
        self.rates.sort(self.sortFunction);
        $("#stat-sort-icon").remove();
        var sortIcon = self.sortDirection > 0 ? ' <i id="stat-sort-icon" class="icon-chevron-up"></i>'
            : ' <i id="stat-sort-icon" class="icon-chevron-down"></i>';
        $("#" + self.sortType).after(sortIcon);
    };
    self.sortFunction = function (left, right) {
        var tLeft, tRight;
        if (self.sortType == self.sortTypes[0]) {
            tLeft = left.Name;
            tRight = right.Name;
        }
        else if (self.sortType == self.sortTypes[1]) {
            tLeft = left.Buy;
            tRight = right.Buy;
        }
        else {
            tLeft = left.Sell;
            tRight = right.Sell;
        }

        if (self.sortDirection > 0) { // -1 DESC, 1 ASC
            var temp = tLeft;
            tLeft = tRight;
            tRight = temp;
        }
        return tLeft == tRight ? 0 : (tLeft > tRight ? -1 : 1);
    };
    self.getRates = function () {
        self.rates([]);
        $("#rtable tbody").after("<tr><td colspan='4'><img src='/images/loading.gif'></td></tr>");
        var sortBy = (self.sortTypes.indexOf(self.sortType) + 1) * self.sortDirection;
        $.post("/rate/getallrates", { Currency: self.currency, City: erm.city().Value, SortBy: sortBy }, function (data) {
            $("#rtable tr:last").remove();
            self.rates(data);
            $('#rtable tbody tr').click(self.showChart);
            self.showChart();
        });
    };
    self.changeCur = function () {
        var $parent = $('#stat-cur-toggle');
        $parent && $parent
            .find('.active')
            .removeClass('active btn-danger');
        $(this).toggleClass('active btn-danger');
        //prevent changing currency from clicking on the same cur
        if (self.currency == $(this).data('cur')) return;
        self.currency == self.currencies[0]
            ? self.currency = self.currencies[1]
            : self.currency = self.currencies[0];
        self.getRates();
    };
    self.showChart = function (event) {
        //hack for IE
        if (!event) event = window.event;
        var bankCode;
        if (typeof event === "undefined" || $(event.currentTarget)[0] == null ||
            typeof (bankCode = $(event.currentTarget)[0].id) === "undefined") {
            if (self.selBank() !== "")
                bankCode = self.selBank().BankCode;
            else return;
        }

        self.selBank(ko.utils.arrayFirst(self.rates(), function(item) {
            return item.BankCode === bankCode;
        }));

        //convert data from server to the format of the highchart API
        var buyData = [], sellData = [];
        var bC = self.selBank().BuyChart;
        var sC = self.selBank().SellChart;
        for (var i = 0; i < bC.length; i++) {
            var jsDate = new Date(bC[i][0]);
            var utcDate = Date.UTC(jsDate.getFullYear(), jsDate.getMonth(), jsDate.getDate(), jsDate.getHours(), jsDate.getMinutes());
            buyData[i] = [utcDate, parseFloat(bC[i][1], 10)];
            sellData[i] = [utcDate, parseFloat(sC[i][1], 10)];
        }
        //show the chart
        Highcharts.setOptions({
            global: { useUTC: false },
            lang: {
                months: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'],
                shortMonths: ['Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн', 'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек'],
                weekdays: ['Воскресенье', 'Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота']
            }
        });
        $('#chart').highcharts({
            title: { text: 'График изменения курса ' + self.currency },
            subtitle: { text: self.selBank().Name },
            credits: { enabled: false },
            exporting: { enabled: false },
            xAxis: { type: 'datetime' },
            yAxis: { title: { text: 'Рубли' } },
            tooltip: {
                formatter: function () {
                    return Highcharts.dateFormat('%A, %e %b %Y', this.x) + '<br/>' +
                        '<span style="font-weight: bold; color:' + this.series.color + '">'
                        + this.series.name + '</span>:<b> ' + erm.money(this.y) + ' руб.</b>';
                }
            },
            series: [{
                name: 'Покупка',
                data: buyData
            }, {
                name: 'Продажа',
                data: sellData
            }]
        });
    };
    self.deleteChart = function () {
        self.selBank("");
        $('#chart').html("");
    };

    self.init = function () {
        if (!self.isInited) {
            self.isInited = true;
            $('span#stat-date').text(erm.mvm.getDate());
            $('#stat-cur-toggle button').click(self.changeCur);
            $('#chart').scrollToFixed({ marginTop: 30 });
        }
        self.deleteChart();
        self.getRates();
    };
};