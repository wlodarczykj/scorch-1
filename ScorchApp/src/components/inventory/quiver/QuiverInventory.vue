<template>
    <div class="card">
    <quiver-detail :quiver="selectedQuiver" :showModal="showDetail" v-on:close="showDetail = false"></quiver-detail>
    <div class="card-header" role="tab" id="quiver">
        <h5 class="mb-0">
        <a data-toggle="collapse" href="#quiverInventory" aria-expanded="false" aria-controls="quiverInventory">
            Quivers and Arrows
        </a>
        </h5>
    </div>
    <div id="quiverInventory" class="collapse" role="tabpanel" aria-labelledby="quiver" data-parent="#accordion">
        <div class="card-body item-list">
        <div v-for="(quiver, index) in quivers" 
                 @click="quiverClick(quiver)" 
                 :key="index" 
                 class="d-flex flex-column list-item border">
                <div class="d-flex justify-content-between">
                    <span class="align-middle">{{ quiver.Name }}</span>
                    <button class="btn btn-primary" @click="equipQuiver(quiver, $event)">
                        <i class="fa fa-level-up" aria-hidden="true"></i>
                    </button>
                </div>
                <div class="projectile-count d-flex flex-column">
                    <div class="projectiles d-flex" v-for="(count, projectile, index) in quiver.Projectiles" :key="index">
                        <strong>{{projectile}} : {{ getArrowCount(count) }}</strong>
                        <div class="d-flex flex-row ml-auto">
                        <button class="projectile-minus btn btn-primary btn-sm" @click="decrementProjectile(count, quiver, $event)"><i class="fa fa-minus"></i></button>                        
                        <button class="projectile-add btn btn-primary btn-sm" @click="incrementProjectile(count, quiver, $event)"><i class="fa fa-plus"></i></button>
                        </div>
                   </div>
                </div>
            </div>
        </div>
    </div>
    </div>
</template>

<script>
import QuiverDetail from './QuiverDetail'

export default {
    name: 'quiver-inventory',
    props: ['characterId', 'quivers'],
    data() {
        return {
            selectedQuiver: {},
            showDetail: false
        }
    },
    methods: {
        quiverClick(quiver) {
            this.selectedQuiver = quiver;
            this.showDetail = true;
        },
        incrementProjectile(projectile, quiver, event) {
            if (event) {
                event.stopPropagation();
            }
            projectile.CurrentAmount++;
            if(projectile.CurrentAmount >= projectile.MaxAmount) {
                projectile.CurrentAmount = projectile.MaxAmount;
            }
            
        },
        decrementProjectile(projectile) {
            if (event) {
                event.stopPropagation();
            }
            projectile.CurrentAmount--;
            if(projectile.CurrentAmount <= 0){
                projectile.CurrentAmount = 0;
            }
        },
        getArrowCount(projectile) {
            return `${projectile.CurrentAmount}/${projectile.MaxAmount}`;
        },
        equipQuiver(quiver, event) {
            if (event) {
                event.stopPropagation();
            }
            this.$emit('equip', quiver);
        }
    },
    components : {
        QuiverDetail
    }
}
</script>

<style lang="scss" scoped>
@import '~styles/shared.scss';
.projectile-count {
    margin-top: 1%;
}
.projectile-add, .projectile-minus {
    margin-left: 1%;
}
.projectiles {
    margin-bottom: 1%;
}
</style>