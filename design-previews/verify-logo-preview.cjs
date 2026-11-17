const { chromium } = require('C:/Users/asbuyruk/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const assert = require('node:assert/strict');
const { pathToFileURL } = require('node:url');
const path = require('node:path');
(async () => {
 const browser = await chromium.launch({headless:true,channel:'msedge'});
 try {
  const page = await browser.newPage({viewport:{width:1200,height:1100}});
  const errors=[]; page.on('pageerror', error=>errors.push(error.message));
  await page.goto(pathToFileURL(path.join(__dirname,'jenga-logo-animations.html')).href);
  assert.equal(await page.locator('.card').count(),7);
  assert.equal(await page.locator('img').evaluateAll(images=>images.every(image=>image.complete&&image.naturalWidth>0)),true);
  assert.ok(await page.locator('#cards').evaluate(el=>el.getAnimations({subtree:true}).length)>20);
  await page.locator('#pause').click();
  assert.equal(await page.locator('#pause').getAttribute('aria-pressed'),'true');
  assert.equal(await page.locator('.wave b').first().evaluate(el=>getComputedStyle(el).animationPlayState),'paused');
  await page.locator('#speed').selectOption('4.8');
  assert.equal(await page.locator('.wave b').first().evaluate(el=>getComputedStyle(el).animationDuration),'4.8s');
  await page.locator('#once').check();
  assert.equal(await page.locator('.stack b').first().evaluate(el=>getComputedStyle(el).animationName),'stack-once');
  await page.locator('#pause').click();
  await page.locator('#speed').selectOption('2.2');
  await page.locator('#replay').click();
  await page.waitForTimeout(3000);
  assert.equal(await page.locator('.stack b').first().evaluate(el=>getComputedStyle(el).opacity),'1');
  assert.equal(await page.locator('.draw path').first().evaluate(el=>getComputedStyle(el).fillOpacity),'1');
  await page.locator('#once').uncheck();
  await page.locator('#speed').selectOption('3.2');
  await page.locator('#replay').click();
  await page.waitForTimeout(1700);
  await page.locator('#pause').click();
  await page.screenshot({path:path.join(__dirname,'jenga-logo-animations.png'),fullPage:true});
  await page.locator('#light').check();
  assert.equal(await page.locator('.stage').first().evaluate(el=>getComputedStyle(el).backgroundColor),'rgb(238, 243, 249)');
  for(const width of [320,640,800,1200]){
   await page.setViewportSize({width,height:1000});
   assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true,'overflow at '+width);
  }
  await page.emulateMedia({reducedMotion:'reduce'});
  assert.equal(await page.locator('.wave b').first().evaluate(el=>getComputedStyle(el).animationName),'none');
  assert.ok((await page.locator('#motion-note').textContent()).length>0);
  await page.locator('#allow-motion').check();
  assert.equal(await page.locator('.wave b').first().evaluate(el=>getComputedStyle(el).animationName),'wave');
  assert.equal(await page.locator('#pause').isEnabled(),true);
  assert.equal(await page.locator('#motion-note').textContent(),'');
  await page.locator('#pause').click();
  await page.waitForTimeout(80);
  const firstTime=await page.locator('.wave b').first().evaluate(el=>el.getAnimations()[0].currentTime);
  await page.waitForTimeout(150);
  const secondTime=await page.locator('.wave b').first().evaluate(el=>el.getAnimations()[0].currentTime);
  assert.ok(secondTime>firstTime,'override should advance animation');
  await page.locator('#allow-motion').uncheck();
  assert.equal(await page.locator('.wave b').first().evaluate(el=>getComputedStyle(el).animationName),'none');
  assert.deepEqual(errors,[]);
  console.log('PASS: 7 examples, embedded images, animation, pause, speed, replay, single play final state, background, 320–1200px layout, reduced motion, no JS errors.');
 } finally {await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
