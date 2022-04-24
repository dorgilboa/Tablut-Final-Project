import { Component } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: 'home.page.html',
  styleUrls: ['home.page.scss'],
})
export class HomePage {

  constructor(private router: Router) {}

  start1v1(){
    let navigationExtras: NavigationExtras = {
      queryParams: {
        compColor: "none"
      }
    };
    this.router.navigate(['/board'], navigationExtras);
  }

  start1vb(){
    let navigationExtras: NavigationExtras = {
      queryParams: {
        compColor: "black"
      }
    };
    this.router.navigate(['/board'], navigationExtras);
  }

  start1vw(){
    let navigationExtras: NavigationExtras = {
      queryParams: {
        compColor: "white"
      }
    };
    this.router.navigate(['/board'], navigationExtras);
  }

}


