angular.module('app', ['ngMaterial', 'chart.js', 'ngStorage'])

    .config(['$httpProvider', function ($httpProvider) {
        //*******************disable catche**********************
        if (!$httpProvider.defaults.headers.get) {
            $httpProvider.defaults.headers.get = {};
        }
        $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
        $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
        $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
        //*******************************************************
    }])

    .config(function($mdDateLocaleProvider) {
        $mdDateLocaleProvider.formatDate = function(date) {
            return moment(date).format("DD.MM.YYYY");
        }
    })

.controller('AppCtrl', ['$scope', '$mdDialog', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', function ($scope, $mdDialog, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http) {

    var getConfig = function () {
        $http.get('../config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
          });
    };
    getConfig();

    var webService = 'CheckIn.asmx';

    var checkUser = function () {
        if ($sessionStorage.userid == "" || $sessionStorage.userid == undefined) {
            $rootScope.currTpl = 'assets/partials/login.html';
            $rootScope.isLogin = false;
        } else {
            $rootScope.currTpl = 'assets/partials/dashboard.html';
            $scope.activeTab = 0;
            $rootScope.isLogin = true;
        }
    }
    checkUser();

    $scope.toggleTpl = function (x, y) {
        $rootScope.currTpl = x;
        $rootScope.activeTab = y;
    };

    $scope.newClient = function () {
        $http({
            url: $rootScope.config.backend + 'Clients.asmx/Init',
            method: "POST",
            data: ""
        })
       .then(function (response) {
           $rootScope.client = JSON.parse(response.data.d);
       },
       function (response) {
           alert(response.data.d)
       });

        $http({
            url: $rootScope.config.backend + 'ClientServices.asmx/Init',
            method: "POST",
            data: ""
        })
       .then(function (response) {
           $rootScope.clientService = JSON.parse(response.data.d);
       },
       function (response) {
           alert(response.data.d)
       });
    }

    $scope.today = new Date();

    $scope.getDateDiff = function (x) {
        var today = new Date();
        var date1 = today;
        var date2 = new Date(x);
        var diffDays = parseInt((date2 - date1) / (1000 * 60 * 60 * 24));
        return diffDays;
    }


    //CheckIn
    if ($localStorage.currentClients == undefined) {
        $scope.currentClients = [];
    } else {
        $scope.currentClients = $localStorage.currentClients
    }
  

    $rootScope.clientsFilter = function (clientType) {
        var obj = $rootScope.clients;
        return function (obj) {
            return obj.isActive === clientType;
        };
    };

    $scope.currentNumberOfClients = function () {
        return $scope.currentClients.length;
    }

    $scope.expirationMembership = function () {
        var count = 0;
        angular.forEach($rootScope.clients, function (value, key) {
            if ($scope.getDateDiff(value.dateTo) < 0) {
                count ++;
            }
        })
        return count;
    }

    $scope.NumberOfNewMembers = function () {
        var count = 0;
        angular.forEach($rootScope.clients, function (value, key) {
            if ($scope.getDateDiff(value.dateFrom) > -30) {
                console.log($scope.getDateDiff(value.dateFrom));
                count++;
            }
        })
        return count;
    }


    $scope.showSaveMessage = false;


    $scope.logout = function () {
        $sessionStorage.userid = "";
        $sessionStorage.username = "";
        $rootScope.isLogin = false;
        $rootScope.currTpl = 'assets/partials/login.html';
    };


    //$scope.newDirector = function (x) {
    //    $http({
    //        url: $rootScope.config.backend + 'Users.asmx/Init',
    //        method: "POST",
    //        data: ""
    //    })
    //    .then(function (response) {
    //        $scope.newUser = JSON.parse(response.data.d);
    //        $rootScope.currTpl = x;
    //    },
    //    function (response) {
    //        alert(response.data.d)
    //    });
    //}

   
}])

.controller('loginCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', function ($scope, $http, $sessionStorage, $window, $rootScope) {
    var webService = 'Users.asmx';
    $scope.login = function (u, p) {
        debugger;
        $http({
            url: $rootScope.config.backend + webService + '/Login',
            method: "POST",
            data: {
                userName: u,
                password: p
            }
        })
        .then(function (response) {
            if (JSON.parse(response.data.d).userName == u) {
                $rootScope.user = JSON.parse(response.data.d);
                $rootScope.loginUser = JSON.parse(response.data.d);
                $sessionStorage.userid = $rootScope.user.userId;
                $sessionStorage.username = $rootScope.user.userName;
                $sessionStorage.admintype = $rootScope.user.adminType;
                $sessionStorage.usergroupid = $rootScope.user.userGroupId;
                $rootScope.isLogin = true;
                $rootScope.currTpl = 'assets/partials/dashboard.html';
            } else {
                $scope.errorLogin = true;
                $scope.errorMesage = 'Pogrešno korisničko ime ili lozinka.'
                //$rootScope.currTpl = 'assets/partials/singup.html';  //<< Only fo first registration
            }
        },
        function (response) {
            $scope.errorLogin = true;
            $scope.errorMesage = 'Korisnik nije pronađen.'
        });
    }
}])

.controller("schedulerCtrl", ['$scope', '$localStorage', '$http', '$rootScope', '$timeout', function ($scope, $localStorage, $http, $rootScope, $timeout) {
    var webService = 'Scheduler.asmx';

    var getRooms = function () {
        $http({
            url: $rootScope.config.backend + 'schoolClasses.asmx/Load',
            method: 'POST',
            data: ''
        })
       .then(function (response) {
           $scope.rooms = JSON.parse(response.data.d);
           if ($scope.rooms.length > 0) {
               $scope.room = $scope.rooms[0].id;
               //  $timeout(function () {
               // showScheduler();
               $scope.getSchedulerByRoom();
               // }, 50);
           } else {
               alert("Unesite školska odjeljenja!");
           }
        
       },
       function (response) {
           alert(response.data.d)
       });
    }
    getRooms();

    $scope.getSchedulerByRoom = function () {
        $http({
            url: $rootScope.config.backend + webService + '/GetSchedulerByRoom',
            method: 'POST',
            data: {room: $scope.room}
        })
       .then(function (response) {
           $rootScope.events = JSON.parse(response.data.d);
           showScheduler();
       },
       function (response) {
           alert(response.data.d)
       });
    }

    var showScheduler = function () {
        YUI().use('aui-scheduler', function (Y) {
            var agendaView = new Y.SchedulerAgendaView();
            var dayView = new Y.SchedulerDayView();
            var weekView = new Y.SchedulerWeekView();
            var monthView = new Y.SchedulerMonthView();
            var eventRecorder = new Y.SchedulerEventRecorder({
                on: {
                    save: function (event) {
                        addEvent(this.getTemplateData(), event);
                      //  alert('Save Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                    },
                    edit: function (event) {
                        addEvent(this.getTemplateData(), event);
                       // editEvent(this.getTemplateData(), event);
                      //  alert('Edit Event:' + this.isNew() + ' --- ' + this.getContentNode().val() + ' --- ' + this.getTemplateData());
                    },
                    delete: function (event) {
                        removeEvent(this.getTemplateData(), event);
                       // alert('Delete Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                       //  Note: The cancel event seems to be buggy and occurs at the wrong times, so I commented it out.
                              },
                    //          cancel: function(event) {
                    //              alert('Cancel Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                    //}
                }
            });

            new Y.Scheduler({
                activeView: weekView,
                boundingBox: '#' + $scope.room, // '#myScheduler',
                date: new Date(),
                eventRecorder: eventRecorder,
                items: $rootScope.events,
                render: true,
                views: [dayView, weekView, monthView, agendaView],
                strings: { agenda: 'Dnevni red', day: 'Dan', month: 'Mjesec', table: 'Tablica', today: 'Danas', week: 'Tjedan', year: 'Godina' },
            }
          );
        });
    }

    var addEvent = function (x, event) {
        $rootScope.events.push({
            //'yuid': event.details[0].newSchedulerEvent._yuid,
            'content': event.details[0].newSchedulerEvent.changed.content,
            'endDate': x.endDate,
            'startDate': x.startDate,
            'room': $scope.room
        });
        var eventObj = {};
        eventObj.content = event.details[0].newSchedulerEvent.changed.content == null ? x.content : event.details[0].newSchedulerEvent.changed.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;
        eventObj.room = $scope.room;

        saveEvent(eventObj);
    }

    var saveEvent = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Delete',
            method: "POST",
            data: '{newEvent:' + JSON.stringify(x) + '}'
        })
        .then(function (response) {
            save(x);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var save = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Save',
            method: "POST",
            data: '{newEvent:' + JSON.stringify(x) + '}'
        })
        .then(function (response) {
            $scope.getSchedulerByRoom();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var removeEvent = function (x, event) {
        var eventObj = {};
        eventObj.content = x.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;
        eventObj.room = $scope.room;
        remove(eventObj);
      //  $scope.getSchedulerByRoom();
      //  load();
    }

    var remove = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Delete', // '../Scheduler.asmx/Delete',
            method: "POST",
            data: '{newEvent:' + JSON.stringify(x) + '}'
        })
        .then(function (response) {
            $scope.getSchedulerByRoom();
        },
        function (response) {
            alert(response.data.d)
        });
    }

}])

.controller('userCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', function ($scope, $http, $sessionStorage, $window, $rootScope) {
    var webService = 'Users.asmx';

    $scope.adminTypes = [
       {
           'value': '0',
           'text': 'Supervizor'
       },
       {
           'value': '1',
           'text': 'Ravnatelj'
       },
       {
           'value': '2',
           'text': 'Voditelj'
       }
    ];

    $scope.userTypes = [
      {
          'value': '0',
          'text': 'Tip korisnika'
      },
      {
          'value': '1',
          'text': 'tip korisnika'
      }
    ];

    $scope.init = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Init', // '../Users.asmx/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            getDirectors();
            $scope.newUser = JSON.parse(response.data.d);
            //var userGroup = JSON.parse(response.data.d);
            $scope.newUser.adminType = 2;
            load();
        },
        function (response) {
            alert(response.data.d)
        });
    }
    $scope.init();

    var getDirectors = function () {
        $http({
            url: $rootScope.config.backend + webService + '/GetUsersByAdminType',
            method: 'POST',
            data: {adminType: 1}
        })
      .then(function (response) {
          $scope.directors = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };

    var load = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Load',
            method: 'POST',
            data: { adminType: $sessionStorage.admintype, userGroupId: $sessionStorage.usergroupid, userId: $sessionStorage.userid }
        })
      .then(function (response) {
          $scope.users = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };
    //load();

    $scope.adminType = function (x) {
        switch (x) {
            case 0:
                return 'Supervizor';
                break;
            case 1:
                return 'Ravnatelj';
                break;
            case 2:
                return 'Voditelj';
                break;
            default:
                return '';
        }
    }

    //$scope.login = function (u, p) {
    //    $http({
    //        url: $rootScope.config.backend + webService + '/Login',
    //        method: "POST",
    //        data: {
    //            userName: u,
    //            password: p
    //        }
    //    })
    //    .then(function (response) {
    //        if (JSON.parse(response.data.d).userName == u) {
    //            $rootScope.user = JSON.parse(response.data.d);
    //            $rootScope.loginUser = $rootScope.user;
    //            $sessionStorage.userid = $rootScope.user.userId;
    //            $sessionStorage.username = $rootScope.user.userName;
    //            $rootScope.isLogin = true;
    //            $rootScope.currTpl = 'assets/partials/dashboard.html';
    //        } else {
    //            $scope.errorLogin = true;
    //            $scope.errorMesage = 'Pogrešno korisničko ime ili lozinka.'
               
    //            //$rootScope.currTpl = 'assets/partials/singup.html';  //<< Only fo first registration
    //        }
    //    },
    //    function (response) {
    //        $scope.errorLogin = true;
    //        $scope.errorMesage = 'Korisnik nije pronađen.'
    //        $rootScope.currTpl = 'assets/partials/login.html';  //<< test
    //    });
    //}

    $scope.singup = function () {
        if ($scope.newUser.password == "" || $scope.passwordConfirm == "") {
            alert("Upišite lozinku.");
            return false;
        }
        if ($scope.newUser.password != $scope.passwordConfirm) {
            alert("Lozinke nisu jednake.");
            return false;
        }

        if ($rootScope.user == undefined) { $rootScope.user = $scope.newUser; }

        $http({
            url: $rootScope.config.backend + webService + '/GetUser',
            method: "POST",
            data: { userId: $scope.newUser.userGroupId } // '{user:' + JSON.stringify($scope.newUser) + '}'  //{ user: $scope.newUser }
        })
        .then(function (response) {
            var userGroup = JSON.parse(response.data.d);
            $scope.newUser.companyName = userGroup.companyName;
            $scope.newUser.address = userGroup.address;
            $scope.newUser.postalCode = userGroup.postalCode;
            $scope.newUser.city = userGroup.city;
            $scope.newUser.country = userGroup.country;
            $scope.newUser.pin = userGroup.pin;
            $scope.newUser.phone = userGroup.phone;
            // alert(response.data.d)


                $http({
                    url: $rootScope.config.backend + webService + '/Singup',
                    method: "POST",
                    data: { user: $scope.newUser } // '{user:' + JSON.stringify($scope.newUser) + '}'  //{ user: $scope.newUser }
                })
            .then(function (response) {
                alert(response.data.d)
            },
            function (response) {
                alert(response.data.d)
            });


        },
        function (response) {
            alert(response.data.d)
        });








        //$scope.newUser.companyName = $rootScope.user.companyName;
        //$scope.newUser.address = $rootScope.user.address;
        //$scope.newUser.postalCode = $rootScope.user.postalCode;
        //$scope.newUser.city = $rootScope.user.city;
        //$scope.newUser.country = $rootScope.user.country;
        //$scope.newUser.pin = $rootScope.user.pin;
        //$scope.newUser.phone = $rootScope.user.phone;
        // $scope.newUser.userGroupId = $rootScope.user.userGroupId;

        //$scope.newUser.activationDate = new Date();
        //$scope.newUser.expirationDate = new Date();
        //$scope.newUser.isActive = 1;
        //$scope.newUser.ipAddress = "";

     

     
    }

    $scope.save = function (x) {
        x.userGroupId = $sessionStorage.userid;
        $http({
            url: $rootScope.config.backend + webService + '/Save',
            method: 'POST',
            data: {x: x} // '{user:' + JSON.stringify($rootScope.user) + '}'
        })
       .then(function (response) {
         //  $rootScope.user = JSON.parse(response.data.d);
           alert(response.data.d);
       },
       function (response) {
           alert(response.data.d)
       });
    }

    $scope.showUser = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/GetUser',
            method: 'POST',
            data: {userId: x}
        })
     .then(function (response) {
         $rootScope.user = JSON.parse(response.data.d);
         $rootScope.currTpl = 'assets/partials/user.html';
     },
     function (response) {
         alert(response.data.d)
     });
    };

    $scope.newDirector = function (x) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.user = JSON.parse(response.data.d);
            $rootScope.user.adminType = 1;
            $rootScope.currTpl = x;
        },
        function (response) {
            alert(response.data.d)
        });
    }


}])

.controller('clientCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', function ($scope, $http, $sessionStorage, $window, $rootScope) {
    var webService = 'Clients.asmx';
    $scope.inactiveServiceClass = function (x) {
        if (getDateDiff(x) > 0) {
            return "";
        } else {
            return "text-muted";
        }
    }

    $scope.activeService = function (x) {
        if (getDateDiff(x) > 0) {
            return true;
        } else {
            return false;
        }
    }

    var getDateDiff = function (x) {
        var today = new Date();
        var date2 = new Date(x);
        var diffDays = parseInt((date2 - today) / (1000 * 60 * 60 * 24));
        return diffDays;
    }

    $scope.init = function () {
        $http({
            url: $rootScope.config.backend + 'ClientServices.asmx/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.clientService = JSON.parse(response.data.d);
            $rootScope.clientService.activationDate = new Date($rootScope.clientService.activationDate);
            $rootScope.clientService.expirationDate = new Date($rootScope.clientService.expirationDate);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    $scope.init();

    var getServices = function () {
        $http({
            url: $rootScope.config.backend + 'Files.asmx/GetFile',
            method: 'POST',
            data: { foldername: 'spartacus', filename: 'services' }
        })
       .then(function (response) {
           $rootScope.services = JSON.parse(response.data.d);
       },
       function (response) {
           alert(response.data.d)
       });

    };
    getServices();


    $scope.currentService = function (idx) {
        $scope.serviceIndex = idx;
       // $scope.quantityIndex = x;
    }

    $scope.currentOption = function (idx) {
        $scope.optionIndex = idx;
    }

    $scope.currentPrice = function (x, idx) {
        $scope.priceIndex = idx;
        $rootScope.clientService.expirationDate = new Date(new Date($rootScope.clientService.activationDate).setDate(new Date($rootScope.clientService.activationDate).getDate() + x.duration));
    }

    var getAllClients = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Load', //'../Clients.asmx/Load',
            method: "POST",
            data: ""
        })
      .then(function (response) {
          $rootScope.clients = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };
    getAllClients();
    
    $scope.save = function () {
        if ($rootScope.client == undefined || $rootScope.client.clientId == undefined) {
            save();
        } else {
            update();
        }
    }

    var save = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Save', // '../Clients.asmx/Save',
            method: "POST",
            data: '{client:' + JSON.stringify($rootScope.client) + '}'
        })
     .then(function (response) {
         $scope.message = response.data.d
         $scope.showSaveMessage = true;
     },
     function (response) {
         $scope.message = response.data.d
         $scope.showSaveMessage = true;
     });
    }

    var update = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Update', // '../Clients.asmx/Update',
            method: "POST",
            data: '{client:' + JSON.stringify($rootScope.client) + '}'
        })
       .then(function (response) {
           $scope.message = response.data.d
           $scope.showSaveMessage = true;
           //  $rootScope.user = JSON.parse(response.data.d);
           // alert(response.data.d);
       },
       function (response) {
           alert(response.data.d)
           $scope.message = response.data.d
           $scope.showSaveMessage = true;
       });
    }

    $scope.showClient = function (x) {
        $rootScope.client = x;
        $rootScope.client.birthDate = new Date($rootScope.client.birthDate);
        $scope.getClientServices(x.clientId);
        //toggleTpl('assets/partials/client.html', 2);
    };

    var toggleTpl = function (x, y) {
        $rootScope.currTpl = x;
        $rootScope.activeTab = y;
    };

    $scope.saveClientService = function (x) {
        x.clientId = $rootScope.client.clientId;
        x.activationDate = x.activationDate || new Date();
        x.expirationDate = x.expirationDate || new Date().setDate(new Date().getDate() + 31);
        x.price = $scope.services[$scope.serviceIndex].options[$scope.optionIndex].prices[$scope.priceIndex].price; // x.price || "";
        x.unit = $scope.services[$scope.serviceIndex].options[$scope.optionIndex].prices[$scope.priceIndex].unit;
        x.quantityLeft = x.quantity;
        $http({
            url: $rootScope.config.backend + 'ClientServices.asmx/Save', // '../ClientServices.asmx/Save',
            method: 'POST',
            data: { clientService: x }
        })
     .then(function (response) {
         $rootScope.clientServices = JSON.parse(response.data.d);
         $scope.message = "Nova usluga je spremljena."
         $scope.showSaveMessage = true;
         //toggleTpl('assets/partials/client.html', 2);
     },
     function (response) {
         alert(response.data.d)
     });
    };

    $scope.editClientCervice = function (x) {
      
        $rootScope.clientService = x;
        $rootScope.clientService.activationDate = new Date(x.activationDate);
        $rootScope.clientService.expirationDate = new Date(x.expirationDate);

    }

    $scope.freezeClientService = function (x) {
        alert('todo');
     //   x.clientId = $rootScope.client.clientId;
     //   x.activationDate = x.activationDate || new Date();
     //   x.expirationDate = x.expirationDate || new Date().setDate(new Date().getDate() + 31);
     //   x.price = $scope.services[$scope.serviceIndex].options[$scope.optionIndex].prices[$scope.priceIndex].price; // x.price || "";
     //   x.unit = $scope.services[$scope.serviceIndex].options[$scope.optionIndex].prices[$scope.priceIndex].unit;

     //   $http({
     //       url: $rootScope.config.backend + 'ClientServices.asmx/Save', // '../ClientServices.asmx/Save',
     //       method: 'POST',
     //       data: { clientService: x }
     //   })
     //.then(function (response) {
     //    $rootScope.clientServices = JSON.parse(response.data.d);
     //    //toggleTpl('assets/partials/client.html', 2);
     //},
     //function (response) {
     //    alert(response.data.d)
     //});
    };



    $scope.getClientServices = function (x) {
        $http({
            url: $rootScope.config.backend + 'ClientServices.asmx/GetClientServices',
            method: 'POST',
            data: { clientId: x }
        })
     .then(function (response) {
         $rootScope.clientServices = JSON.parse(response.data.d);
         $rootScope.servicesCount = $rootScope.clientServices.length.toString();
         toggleTpl('assets/partials/client.html', 2);
     },
     function (response) {
         alert(response.data.d)
     });
    };

    //todo
    $scope.getTotal = function() {
        angular.forEach($rootScope.clientServices, function (value, key) {
            //if ($scope.getDateDiff(value.dateTo) < 0) {
            //    count ++;
            //}
        })
    }

}])

    //HRZ Controllers

.controller('hrzCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog) {
    $rootScope.newTpl = 'assets/partials/realizations.html',
    $scope.toggleNewTpl = function (x, y) {
            $rootScope.newTpl = x;
    };

    var getSchools = function () {
        $http({
            url: $rootScope.config.backend + 'SchoolClasses.asmx/GetSchools',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.schools = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    };
    getSchools();

    var getLeaders = function () {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Load',
            method: "POST",
            data: { adminType: $sessionStorage.admintype, userGroupId: $sessionStorage.usergroupid, userId: $sessionStorage.userid }
         //   data: ""
        })
      .then(function (response) {
          $rootScope.leaders = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };
    getLeaders();

    var getAllClients = function () {
        $http({
            url: $rootScope.config.backend + 'Clients.asmx/Load',
            method: "POST",
            data: ""
        })
      .then(function (response) {
          $rootScope.clients = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };
    getAllClients();

    
    //$scope.openPopup = function (tmpl) {
    //    $mdDialog.show({
    //        controller: $scope.popupCtrl, // RealizationCtrl,
    //        templateUrl: 'assets/partials/popup/' + tmpl + '.html',
    //        parent: angular.element(document.body),
    //        targetEvent: '',
    //        clickOutsideToClose: true,
    //        fullscreen: $scope.customFullscreen // Only for -xs, -sm breakpoints.
    //    })
    //    .then(function (answer) {
    //        $scope.status = 'You said the information was "' + answer + '".';
    //    }, function () {
    //        $scope.status = 'You cancelled the dialog.';
    //    });
    //};


  

    //$scope.popupCtrl = function ($scope, $mdDialog) {
    //    $scope.g = {};
    //    $scope.hide = function () {
    //        $mdDialog.hide();
    //    };
    //    $scope.cancel = function () {
    //        $mdDialog.cancel();
    //    };
    //    $scope.save = function (x) {
    //        alert('todo');
    //        $mdDialog.hide();
    //        return;
    //    }
    //}

}])

.controller('schoolClassesCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog) {
    var webService = 'SchoolClasses.asmx';
    
    //$scope.toggleNewTpl = function (x) {
    //    $rootScope.newTpl = x;
    //};
    $scope.g = [];
    var init = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    init();

    var getSchools = function () {
        $http({
            url: $rootScope.config.backend + webService + '/GetSchools',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.schools = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var load = function () {
        $http({
            url: $rootScope.config.backend + webService +'/Load', // '../Clients.asmx/Load',
            method: "POST",
            data: ""
        })
      .then(function (response) {
          $scope.g = JSON.parse(response.data.d);
          getSchools();
      },
      function (response) {
          alert(response.data.d)
      });
    };
    load();

    var save = function (d) {
        $http({
            url: $rootScope.config.backend + webService + '/Save', // '../Clients.asmx/Save',
            method: "POST",
            data: '{ x:' +JSON.stringify(d) + '}'
        })
     .then(function (response) {
         load();
         alert(response.data.d);
        // $scope.showSaveMessage = true;
     },
     function (response) {
         alert(response.data.d)
         //$scope.message = response.data.d
         //$scope.showSaveMessage = true;
     });
    }

    $scope.get = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Get',
            method: "POST",
            data: { id: x.id }
        })
      .then(function (response) {
          $scope.d = JSON.parse(response.data.d);
          $scope.openPopup();
      },
      function (response) {
          alert(response.data.d)
      });
    };

    var remove = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Delete',
            method: "POST",
            data: { x: x }
        })
      .then(function (response) {
          load();
          // alert(response.data.d);
          // alert('Izbrisano.');
      },
      function (response) {
          alert(response.data.d)
      });
    };

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
              .title('Dali želite izbrisati odjeljenje?')
              .textContent('Br. ' + x.id + ', ' + x.school + ', ' + x.schoolClass)
              .targetEvent(x)
              .ok('DA!')
              .cancel('NE');

        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    $scope.openPopup = function () {
        init();
        $mdDialog.show({
            controller: $scope.popupCtrl, // RealizationCtrl,
            templateUrl: 'assets/partials/popup/schoolclass.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
            d: $scope.d,
            leaders: $rootScope.leaders,
            schools: $rootScope.schools
        })
        .then(function (answer) {
            $scope.status = 'You said the information was "' + answer + '".';
        }, function () {
            $scope.status = 'You cancelled the dialog.';
        });
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, leaders, schools) {
        $scope.d = d;
        $scope.leaders = leaders;
        $scope.schools = schools;
        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.save = function (x) {
            $scope.d.school = $scope.searchText;
            save($scope.d);
            console.log($scope.d);
            $mdDialog.hide();
            return;
        }
    }

}])

.controller('realizationsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog) {
    var webService = 'Realizations.asmx';
    $scope.toggleNewTpl = function (x) {
        $rootScope.newTpl = x;
    };
    $scope.g = [];
    var init = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
            load();
        },
        function (response) {
            alert(response.data.d)
        });
    }
    init();

    var load = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Load',
            method: "POST",
            data: ""
        })
      .then(function (response) {
          $scope.g = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };

    $scope.girType = function (x) {
        switch (x) {
            case 0:
                return 'Sportska poduka';
                break;
            case 1:
                return 'Povremena animacija';
                break;
            default:
                return '';
        }
    };

    $scope.newRealization = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
            $scope.openPopup();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.createPdf = function () {
        var fileName = 'realizacija';
        $http({
            url: $rootScope.config.backend + 'PrintPdf.asmx/RealizationsPdf',
            method: "POST",
            data: { foldername: $rootScope.config.sitename, filename: fileName, json: JSON.stringify($scope.g) }
        })
        .then(function (response) {
            window.open($rootScope.config.backend + 'UsersFiles/' + $rootScope.config.sitename + '/pdf/' + fileName + '.pdf', + '_blank');
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var save = function (d) {
        d.date = d.date.toLocaleString();
        $http({
            url: $rootScope.config.backend + webService + '/Save',
            method: "POST",
            data: '{ x:' + JSON.stringify(d) + '}'
        })
     .then(function (response) {
         load();
        // alert(response.data.d);
         // $scope.showSaveMessage = true;
     },
     function (response) {
         alert(response.data.d)
         //$scope.message = response.data.d
         //$scope.showSaveMessage = true;
     });
    }

    $scope.get = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Get',
            method: "POST",
            data: { id: x.id }
        })
      .then(function (response) {
          $scope.d = JSON.parse(response.data.d);
          $scope.openPopup();
      },
      function (response) {
          alert(response.data.d)
      });
    };

    var remove = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Delete',
            method: "POST",
            data: { x: x }
        })
      .then(function (response) {
          load();
          // alert(response.data.d);
         // alert('Izbrisano.');
      },
      function (response) {
          alert(response.data.d)
      });
    };

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
              .title('Dali želite izbrisati unos?')
              .textContent('Br. ' + x.id + ', ' + x.school + ', ' + x.schoolClass)
              .ariaLabel('Lucky day')
              .targetEvent(x)
              .ok('DA!')
              .cancel('NE');

        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    $scope.openPopup = function () {
        $mdDialog.show({
            controller: $scope.popupCtrl, // RealizationCtrl,
            templateUrl: 'assets/partials/popup/realization.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
            d: $scope.d,
            leaders: $rootScope.leaders,
            schools: $rootScope.schools
        })
        .then(function (answer) {
        }, function () {
        });
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, leaders, schools, $http) {
        $scope.d = d;
        $scope.d.date = new Date($scope.d.date);
        $scope.schools = schools;
        $scope.leaders = leaders;
        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.save = function (x) {
          //  $scope.d.date = new Date($scope.d.date.setHours(0, 0, 0, 0));
            save($scope.d);
            $mdDialog.hide();
            return;
        }

        $scope.getSchoolClasses = function (x) {
            $http({
                url: $rootScope.config.backend + 'SchoolClasses.asmx/GetSchoolClassesBySchool',
                method: "POST",
                data: { school: x }
            })
           .then(function (response) {
               $scope.schoolClasses = JSON.parse(response.data.d);
           },
           function (response) {
               alert(response.data.d)
           });
        };
        $scope.getSchoolClasses($scope.d.school);

        $scope.getLeaders = function (x) {
            if (x != null) {
                $http({
                    url: $rootScope.config.backend + 'SchoolClasses.asmx/GetLeadersBySchoolClass',
                    method: "POST",
                    data: { id: x }
                })
               .then(function (response) {
                   $scope.leaders = JSON.parse(response.data.d);
               },
               function (response) {
                   alert(response.data.d)
               });
            }
        };
        $scope.getLeaders($scope.d.schoolClassId);

        $scope.getStudents = function (x) {
            $http({
                url: $rootScope.config.backend + 'SchoolClasses.asmx/GetStudentsBySchoolClass',
                method: "POST",
                data: { id: x }
            })
           .then(function (response) {
               $scope.d.students = JSON.parse(response.data.d);
           },
           function (response) {
               alert(response.data.d)
           });
        }

        $scope.toogleAll = function () {
            angular.forEach($scope.d.students, function (value, key) {
                $scope.d.students[key].isSelected = $scope.selectAll;
            })
            $scope.checkStudent();
        }

        $scope.checkStudent = function () {
            $scope.selectedStudents = [];
            angular.forEach($scope.d.students, function (value, key) {
                if ($scope.d.students[key].isSelected == true) {
                    $scope.selectedStudents.push($scope.d.students[key].id)
                }
             //   $scope.d.students = JSON.stringify($scope.selectedStudents);
            })
        }


    }


    //Test html2Canvas
    //$scope.testHtml2Canvas = function () {
    //    html2canvas(document.getElementById("testdiv"), {
    //        onrendered: function (canvas) {
    //            document.body.appendChild(canvas);
    //        }
    //    });
    //}

    //Test jsPdf
    $scope.testPdf = function () {
        var pdf = new jsPDF('p', 'pt', 'a4');  //One of "portrait" or "landscape" (or shortcuts "p" (Default), "l")
        var options = {
            pagesplit: true
        };

        $("#printr").show();
        pdf.addHTML($("#printr"), 0, 0, options, function () {
            $("#printr").hide();
            //  pdf.save("realizacija.pdf");
          //  pdf.autoPrint();
            pdf.output("dataurlnewwindow"); // this opens a new popup,  after this the PDF opens the print window view but there are browser inconsistencies with how this is handled
        });
    }

}])

.controller('studentsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog) {
    var webService = 'Students.asmx';
    $scope.toggleNewTpl = function (x) {
        $rootScope.newTpl = x;
    };
    $scope.g = [];
  //  $scope.date = new Date(new Date().setHours(0, 0, 0, 0));
    var init = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    init();

    var load = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Load', // '../Clients.asmx/Load',
            method: "POST",
            data: ""
        })
      .then(function (response) {
          $scope.g = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };
    load();

    var save = function (d) {
        d.birthDate = d.birthDate.toLocaleString();
        $http({
            url: $rootScope.config.backend + webService + '/Save',
            method: "POST",
            data: '{ x:' + JSON.stringify(d) + '}'
        })
     .then(function (response) {
         load();
        // alert(response.data.d);
         // $scope.showSaveMessage = true;
     },
     function (response) {
         alert(response.data.d)
         //$scope.message = response.data.d
         //$scope.showSaveMessage = true;
     });
    }

    $scope.newStudent = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
            $scope.openPopup();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.get = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Get',
            method: "POST",
            data: { id: x.id }
        })
      .then(function (response) {
          $scope.d = JSON.parse(response.data.d);
          $scope.openPopup();
      },
      function (response) {
          alert(response.data.d)
      });
    };

    var remove = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/Delete',
            method: "POST",
            data: { x: x }
        })
      .then(function (response) {
          load();
      },
      function (response) {
          alert(response.data.d)
      });
    };

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
              .title('Dali želite izbrisati člana?')
              .textContent('Br. ' + x.id + ', ' + x.firstName + ', ' + x.lastName)
              .targetEvent(x)
              .ok('DA!')
              .cancel('NE');

        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    $scope.openPopup = function () {
        $mdDialog.show({
            controller: $scope.popupCtrl, // RealizationCtrl,
            templateUrl: 'assets/partials/popup/student.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
            d: $scope.d,
            schools: $rootScope.schools
        })
        //.then(function (answer) {
        //    $scope.status = 'You said the information was "' + answer + '".';
        //}, function () {
        //    $scope.status = 'You cancelled the dialog.';
        //});
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, schools) {
        $scope.d = d;
        $scope.schools = schools;
        $scope.d.birthDate = new Date($scope.d.birthDate);
        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.save = function () {
            save($scope.d);
            console.log($scope.d);
            $mdDialog.hide();
            return;
        }

        $scope.getSchoolClasses = function (x) {
            $http({
                url: $rootScope.config.backend + 'SchoolClasses.asmx/GetSchoolClassesBySchool',
                method: "POST",
                data: { school: x }
            })
           .then(function (response) {
               $scope.schoolClasses = JSON.parse(response.data.d);
           },
           function (response) {
               alert(response.data.d)
           });
        };
        $scope.getSchoolClasses($scope.d.school);

        $scope.getTeachers = function (x) {
            $http({
                url: $rootScope.config.backend + 'SchoolClasses.asmx/GetTeachersBySchoolClass',
                method: "POST",
                data: { id: x }
            })
           .then(function (response) {
               $scope.teachers = JSON.parse(response.data.d);
           },
           function (response) {
               alert(response.data.d)
           });
        };
        $scope.getTeachers($scope.d.schoolClassId);


    }

}])

.controller("messagesCtrl", ['$scope', '$http', '$timeout', '$rootScope', function ($scope, $http, $timeout, $rootScope) {
    var webService = 'MailMessages.asmx';

    var init = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.mail = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    init();

    var load = function () {
        $http({
            url: $rootScope.config.backend + webService + '/Load',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.messages = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    load();

    $scope.getGroups = function () {
        $http({
            url: $rootScope.config.backend + webService + '/GetGroups',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.groups = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    $scope.getGroups();

    $scope.toogleAll = function () {
        angular.forEach($scope.groups, function (value, key) {
            $scope.groups[key].isSelected = $scope.selectAll;
        })
    }

    $scope.send = function () {
        $http({
            url: $rootScope.config.backend + webService + '/SendNewMail',
            method: 'POST',
            data: { mail: $scope.mail, groups: $scope.groups, sitename: $rootScope.config.sitename }
        })
     .then(function (response) {
         load();
         $scope.message = response.data.d;
         $scope.showSaveMessage = true;
     },
     function (response) {
         alert(response.data.d)
     });
    }

    $scope.show = function (x) {
        $http({
            url: $rootScope.config.backend + webService + '/GetMessage',
            method: "POST",
            data: { mail: x }
        })
        .then(function (response) {
            $scope.mail = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getMailSettings = function () {
        $http({
            url: $rootScope.config.backend + 'Files.asmx/GetFile',
            method: 'POST',
            data: { foldername: $rootScope.config.sitename, filename: 'mailsettings' }
        })
       .then(function (response) {
           $scope.mailSettings = JSON.parse(response.data.d);
       },
       function (response) {
           alert(response.data.d)
       });
    };
    getMailSettings();

    $scope.saveMailSettings = function () {
        saveJsonToFile($rootScope.config.sitename, 'mailsettings', $scope.mailSettings);
    }

    var saveJsonToFile = function (foldername, filename, json) {
        $http({
            url: $rootScope.config.backend + 'Files.asmx/SaveJsonToFile',
            method: "POST",
            data: { foldername: foldername, filename: filename, json: JSON.stringify(json) }
        })
        .then(function (response) {
            $scope.mailSettings = JSON.parse(response.data.d);
            alert('Spremljeno.');
        },
        function (response) {
            alert(response.data.d)
        });
    }


}])


;