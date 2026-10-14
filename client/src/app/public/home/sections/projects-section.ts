import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProjectListItem } from '../../../core/models';
import { TranslatePipe } from '../../../core/i18n/i18n.pipes';
import { Icon } from '../../../shared/icon';
import { ProjectCard } from '../../../shared/project-card';
import { RevealDirective } from '../../../shared/reveal.directive';

@Component({
  selector: 'app-projects-section',
  imports: [RouterLink, TranslatePipe, Icon, ProjectCard, RevealDirective],
  template: `
    <section id="projects" class="section">
      <div class="container">
        <div class="head" appReveal>
          <div class="section-head">
            <span class="kicker">{{ 'projects.kicker' | t }}</span>
            <h2>{{ 'projects.title' | t }}</h2>
            <p>{{ 'projects.subtitle' | t }}</p>
          </div>
          <a routerLink="/projects" class="btn">{{ 'projects.all' | t }} <app-icon name="arrow-right" /></a>
        </div>

        <div class="grid">
          @for (project of projects(); track project.id) {
            <app-project-card [project]="project" appReveal />
          }
        </div>
      </div>
    </section>
  `,
  styles: `
    .head { display: flex; align-items: flex-end; justify-content: space-between; gap: 24px; flex-wrap: wrap; margin-bottom: 44px; }
    .head .section-head { margin-bottom: 0; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px; }
    @media (max-width: 400px) { .grid { grid-template-columns: 1fr; } }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectsSection {
  readonly projects = input<ProjectListItem[]>([]);
}
