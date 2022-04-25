import { Component } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';

/// routing component between human vs human oage and playing vs AI
@Component({
  selector: 'app-home',
  templateUrl: 'home.page.html',
  styleUrls: ['home.page.scss'],
})
export class HomePage {

  constructor(private router: Router) {}

  start1v1(){
    /// two human on same machine
    let navigationExtras: NavigationExtras = {
      queryParams: {
        compColor: "none"
      }
    };
    this.router.navigate(['/board'], navigationExtras);
  }

  start1vb(){
    /// human vs black AI
    let navigationExtras: NavigationExtras = {
      queryParams: {
        compColor: "black"
      }
    };
    this.router.navigate(['/board'], navigationExtras);
  }

  start1vw(){
    /// human vs white AI
    let navigationExtras: NavigationExtras = {
      queryParams: {
        compColor: "white"
      }
    };
    this.router.navigate(['/board'], navigationExtras);
  }

}


